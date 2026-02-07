using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.DailyReadsModule.Domain.Entities;

namespace PureTCOWebApp.Features.DailyReadsModule.Endpoints.QuizEndpoints;

public record QuizChoiceDto(string Choice, string Answer);

public record CreateQuizQuestionRequest(
    int QuestionSeq,
    string Question,
    string CorrectAnswer,
    List<QuizChoiceDto> Choices
);

public class CreateQuizQuestionValidator : AbstractValidator<CreateQuizQuestionRequest>
{
    public CreateQuizQuestionValidator()
    {
        RuleFor(x => x.QuestionSeq)
            .GreaterThan(0).WithMessage("Question Seq must be greater than 0.");

        RuleFor(x => x.Question)
            .NotEmpty().WithMessage("Question is required.")
            .MaximumLength(1000).WithMessage("Question must not exceed 1000 characters.");

        RuleFor(x => x.CorrectAnswer)
            .NotEmpty().WithMessage("Correct Answer is required.")
            .Must(x => new[] { "A", "B", "C", "D", "E" }.Contains(x.ToUpper()))
            .WithMessage("Correct Answer must be A, B, C, D, or E.");

        RuleFor(x => x.Choices)
            .NotEmpty().WithMessage("Choices are required.")
            .Must(x => x.Count >= 2).WithMessage("At least 2 choices are required.")
            .Must(x => x.Count <= 5).WithMessage("Maximum 5 choices allowed.")
            .Must(x => x.Select(c => c.Choice.ToUpper()).Distinct().Count() == x.Count)
            .WithMessage("Duplicate choices are not allowed.");

        RuleForEach(x => x.Choices).ChildRules(choice =>
        {
            choice.RuleFor(x => x.Choice)
                .NotEmpty().WithMessage("Choice is required.")
                .Must(x => new[] { "A", "B", "C", "D", "E" }.Contains(x.ToUpper()))
                .WithMessage("Choice must be A, B, C, D, or E.");

            choice.RuleFor(x => x.Answer)
                .NotEmpty().WithMessage("Answer is required.")
                .MaximumLength(500).WithMessage("Answer must not exceed 500 characters.");
        });

        RuleFor(x => x)
            .Must(x => x.Choices.Any(c => c.Choice.ToUpper() == x.CorrectAnswer.ToUpper()))
            .WithMessage("Correct Answer must match one of the choices.");
    }
}

public class CreateQuizQuestionEndpoint(
    ApplicationDbContext dbContext,
    UnitOfWork unitOfWork
) : Endpoint<CreateQuizQuestionRequest, ApiResponse>
{
    public override void Configure()
    {
        Post("{dailyReadId}/quiz");
        Group<DailyReadsEndpointGroup>();
        Roles("admin");
    }

    public override async Task HandleAsync(CreateQuizQuestionRequest req, CancellationToken ct)
    {
        var dailyReadId = Route<int>("dailyReadId");

        var dailyRead = await dbContext.DailyReads.FindAsync([dailyReadId], ct);
        if (dailyRead == null)
        {
            await Send.ResultAsync(TypedResults.NotFound<ApiResponse>((Result)CrudDomainError.NotFound("DailyRead", dailyReadId)));
            return;
        }

        // Check if question seq already exists
        var exists = await dbContext.QuizQuestions
            .AnyAsync(q => q.DailyReadId == dailyReadId && q.QuestionSeq == req.QuestionSeq, ct);

        if (exists)
        {
            await Send.ResultAsync(TypedResults.Conflict<ApiResponse>(
                Result.Failure(new Error("QuestionSeqExists", $"Question Seq {req.QuestionSeq} already exists for this Daily Read."))));
            return;
        }

        var quizQuestion = QuizQuestion.Create(
            dailyReadId,
            req.QuestionSeq,
            req.Question.Trim(),
            req.CorrectAnswer.ToUpper().Trim()
        );

        foreach (var choiceDto in req.Choices)
        {
            quizQuestion.AddChoice(choiceDto.Choice.ToUpper().Trim(), choiceDto.Answer.Trim());
        }

        await dbContext.AddAsync(quizQuestion, ct);
        var result = await unitOfWork.SaveChangesAsync(ct);

        if (result.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
            return;
        }

        await Send.OkAsync(Result.Success(), ct);
    }
}
