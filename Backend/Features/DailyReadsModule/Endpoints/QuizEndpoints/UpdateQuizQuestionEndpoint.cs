using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.DailyReadsModule.Domain.Entities;

namespace PureTCOWebApp.Features.DailyReadsModule.Endpoints.QuizEndpoints;

public record UpdateQuizQuestionRequest(
    string Question,
    string CorrectAnswer,
    List<QuizChoiceDto> Choices
);

public class UpdateQuizQuestionValidator : AbstractValidator<UpdateQuizQuestionRequest>
{
    public UpdateQuizQuestionValidator()
    {
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

public class UpdateQuizQuestionEndpoint(
    ApplicationDbContext dbContext,
    UnitOfWork unitOfWork
) : Endpoint<UpdateQuizQuestionRequest, ApiResponse>
{
    public override void Configure()
    {
        Put("{dailyReadId}/quiz/{questionSeq}");
        Group<DailyReadsEndpointGroup>();
        Roles("admin");
    }

    public override async Task HandleAsync(UpdateQuizQuestionRequest req, CancellationToken ct)
    {
        var dailyReadId = Route<int>("dailyReadId");
        var questionSeq = Route<int>("questionSeq");

        var quizQuestion = await dbContext.QuizQuestions
            .Include(q => q.Choices)
            .FirstOrDefaultAsync(q => q.DailyReadId == dailyReadId && q.QuestionSeq == questionSeq, ct);

        if (quizQuestion == null)
        {
            await Send.ResultAsync(TypedResults.NotFound<ApiResponse>(
                Result.Failure(new Error("QuestionNotFound", $"Quiz Question with Seq {questionSeq} not found."))));
            return;
        }

        quizQuestion.Update(req.Question.Trim(), req.CorrectAnswer.ToUpper().Trim());
        quizQuestion.ClearChoices();
        
        foreach (var choiceDto in req.Choices)
        {
            quizQuestion.AddChoice(choiceDto.Choice.ToUpper().Trim(), choiceDto.Answer.Trim());
        }

        var result = await unitOfWork.SaveChangesAsync(ct);

        if (result.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
            return;
        }

        await Send.OkAsync(Result.Success(), ct);
    }
}
