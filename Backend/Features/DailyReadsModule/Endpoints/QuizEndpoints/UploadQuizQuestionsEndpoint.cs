using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Core.Upload;
using PureTCOWebApp.Core.Upload.Attributes;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.DailyReadsModule.Domain.Entities;

namespace PureTCOWebApp.Features.DailyReadsModule.Endpoints.QuizEndpoints;

public class QuizQuestionUploadDtoValidator : AbstractValidator<QuizQuestionUploadDto>
{
    public QuizQuestionUploadDtoValidator()
    {
        RuleFor(x => x.QuizNumber)
            .NotEmpty().WithMessage("Quiz Number is required.");

        RuleFor(x => x.Question)
            .NotEmpty().WithMessage("Question is required.");

        RuleFor(x => x.OptionA)
            .NotEmpty().WithMessage("Option A is required.");

        RuleFor(x => x.OptionB)
            .NotEmpty().WithMessage("Option B is required.");

        RuleFor(x => x.CorrectOption)
            .NotEmpty().WithMessage("Correct Option is required.")
            .Must(x => x != null && new[] { "A", "B", "C", "D", "E" }.Contains(x.ToUpper()))
            .WithMessage("Correct Option must be A, B, C, D, or E.");
    }
}

public class UploadQuizQuestionsRequest
{
    public required IFormFile File { get; set; }
}

public class QuizQuestionUploadDto
{
    [ExcelColumn("A", "QuizNumber")] public int? QuizNumber { get; set; }
    [ExcelColumn("B", "Question")] public string? Question { get; set; }
    [ExcelColumn("C", "OptionA")] public string? OptionA { get; set; }
    [ExcelColumn("D", "OptionB")] public string? OptionB { get; set; }
    [ExcelColumn("E", "OptionC")] public string? OptionC { get; set; }
    [ExcelColumn("F", "OptionD")] public string? OptionD { get; set; }
    [ExcelColumn("G", "OptionE")] public string? OptionE { get; set; }
    [ExcelColumn("H", "CorrectOption")] public string? CorrectOption { get; set; }
}

public class UploadQuizQuestionsEndpoint(
    ApplicationDbContext dbContext,
    UnitOfWork unitOfWork
) : Endpoint<UploadQuizQuestionsRequest, ApiResponse>
{
    public override void Configure()
    {
        Post("{dailyReadId}/quiz/upload");
        Group<DailyReadsEndpointGroup>();
        Roles("admin");
        AllowFileUploads();
    }

    public override async Task HandleAsync(UploadQuizQuestionsRequest req, CancellationToken ct)
    {
        var dailyReadId = Route<int>("dailyReadId");

        var dailyRead = await dbContext.DailyReads.FindAsync([dailyReadId], ct);
        if (dailyRead == null)
        {
            await Send.ResultAsync(TypedResults.NotFound<ApiResponse>((Result)CrudDomainError.NotFound("DailyRead", dailyReadId)));
            return;
        }

        var validateResult = ExcelHelper.ValidateExcelFile(req.File);
        if (validateResult.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(validateResult));
            return;
        }

        var fileBytes = new byte[req.File.Length];
        using (var stream = req.File.OpenReadStream())
        {
            await stream.ReadExactlyAsync(fileBytes, 0, (int)req.File.Length, ct);
        }

        var structureValidationResult = ExcelHelper.ValidateColumnStructure<QuizQuestionUploadDto>(fileBytes);
        if (structureValidationResult.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(structureValidationResult));
            return;
        }

        List<QuizQuestionUploadDto> rows;
        try
        {
            rows = ExcelHelper.ParseExcelData<QuizQuestionUploadDto>(fileBytes);
        }
        catch (Exception ex)
        {
            var parseError = BulkResult.Failure(ExcelDomainError.OneOrMoreValidationError)
                .SetErrors([new Error("ExcelParseError", $"Invalid Excel File: {ex.Message}")]);
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(parseError));
            return;
        }

        var results = ValidateRows(rows);
        if (results.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(results));
            return;
        }

        var existingQuestions = await dbContext.QuizQuestions
            .Where(q => q.DailyReadId == dailyReadId)
            .ToListAsync(ct);
        dbContext.RemoveRange(existingQuestions);

        foreach (var dto in rows)
        {
            if (!dto.QuizNumber.HasValue || string.IsNullOrWhiteSpace(dto.Question))
                continue;

            var quizQuestion = QuizQuestion.Create(
                dailyReadId,
                dto.QuizNumber.Value,
                dto.Question.Trim(),
                dto.CorrectOption!.Trim()
            );

            if (!string.IsNullOrWhiteSpace(dto.OptionA))
                quizQuestion.AddChoice("A", dto.OptionA.Trim());
            if (!string.IsNullOrWhiteSpace(dto.OptionB))
                quizQuestion.AddChoice("B", dto.OptionB.Trim());
            if (!string.IsNullOrWhiteSpace(dto.OptionC))
                quizQuestion.AddChoice("C", dto.OptionC.Trim());
            if (!string.IsNullOrWhiteSpace(dto.OptionD))
                quizQuestion.AddChoice("D", dto.OptionD.Trim());
            if (!string.IsNullOrWhiteSpace(dto.OptionE))
                quizQuestion.AddChoice("E", dto.OptionE.Trim());

            await dbContext.AddAsync(quizQuestion, ct);
        }

        var result = await unitOfWork.SaveChangesAsync(ct);
        if (result.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
            return;
        }

        await Send.OkAsync((ApiResponse)results, ct);
    }

    private static BulkResult ValidateRows(ICollection<QuizQuestionUploadDto> rows)
    {
        ICollection<Result> results = [];
        var idx = 2;
        var uniqueNumbers = new HashSet<int>();
        var validator = new QuizQuestionUploadDtoValidator();

        foreach (var row in rows)
        {
            // Normalize CorrectOption to uppercase
            if (!string.IsNullOrWhiteSpace(row.CorrectOption))
            {
                row.CorrectOption = row.CorrectOption.ToUpper().Trim();
            }

            var validation = validator.Validate(row);

            foreach (var failure in validation.Errors)
            {
                var cell = ResolveExcelColumn(failure.PropertyName, idx);
                results.Add(ExcelDomainError.ValidationError(cell, failure.ErrorMessage));
            }

            if (row.QuizNumber.HasValue)
            {
                if (!uniqueNumbers.Add(row.QuizNumber.Value))
                {
                    results.Add(ExcelDomainError.ValidationError($"A{idx}", "Quiz Number must be unique."));
                }
            }

            idx++;
        }

        if (results.Any(r => r.IsFailure))
        {
            return BulkResult
                .Failure(ExcelDomainError.OneOrMoreValidationError)
                .SetErrors([.. results.Where(e => e.IsFailure).Select(e => e.Error)]);
        }

        return BulkResult.Success();
    }

    private static string ResolveExcelColumn(string? propertyName, int rowIndex)
    {
        var prop = propertyName?.ToLowerInvariant() ?? "";

        if (prop.Contains("quiznumber")) return $"A{rowIndex}";
        if (prop.Contains("question")) return $"B{rowIndex}";
        if (prop.Contains("optiona")) return $"C{rowIndex}";
        if (prop.Contains("optionb")) return $"D{rowIndex}";
        if (prop.Contains("optionc")) return $"E{rowIndex}";
        if (prop.Contains("optiond")) return $"F{rowIndex}";
        if (prop.Contains("optione")) return $"G{rowIndex}";
        if (prop.Contains("correctoption")) return $"H{rowIndex}";

        return $"Unknown{rowIndex}";
    }
}
