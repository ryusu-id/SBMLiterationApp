using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.DailyReadsModule.Domain.Entities;

namespace PureTCOWebApp.Features.DailyReadsModule.Endpoints.QuizEndpoints;

public class SubmitQuizAnswerRequestValidator : AbstractValidator<SubmitQuizAnswerRequest>
{
    public SubmitQuizAnswerRequestValidator()
    {
        RuleFor(x => x.Answers)
            .NotEmpty().WithMessage("Answers are required.");

        RuleForEach(x => x.Answers).ChildRules(answer =>
        {
            answer.RuleFor(x => x.QuestionSeq)
                .GreaterThan(0).WithMessage("Question Seq must be greater than 0.");

            answer.RuleFor(x => x.Answer)
                .NotEmpty().WithMessage("Answer is required.")
                .Must(x => new[] { "A", "B", "C", "D", "E" }.Contains(x.ToUpper()))
                .WithMessage("Answer must be A, B, C, D, or E.");
        });
    }
}

public record QuizAnswerDto(int QuestionSeq, string Answer);

public record SubmitQuizAnswerRequest(List<QuizAnswerDto> Answers);

public class SubmitQuizAnswerEndpoint(
    ApplicationDbContext dbContext,
    UnitOfWork unitOfWork
) : Endpoint<SubmitQuizAnswerRequest, ApiResponse>
{
    public override void Configure()
    {
        Post("{dailyReadId}/quiz/submit");
        Group<DailyReadsEndpointGroup>();
    }

    public override async Task HandleAsync(SubmitQuizAnswerRequest req, CancellationToken ct)
    {
        var dailyReadId = Route<int>("dailyReadId");
        var userId = int.Parse(User.FindFirst("sub")!.Value);

        var dailyRead = await dbContext.DailyReads.FindAsync([dailyReadId], ct);
        if (dailyRead == null)
        {
            await Send.ResultAsync(TypedResults.NotFound<ApiResponse>((Result)CrudDomainError.NotFound("DailyRead", dailyReadId)));
            return;
        }

        var existingAnswers = await dbContext.QuizAnswers
            .Where(a => a.UserId == userId && a.DailyReadId == dailyReadId)
            .ToDictionaryAsync(a => a.QuestionSeq, ct);

        foreach (var answerDto in req.Answers)
        {
            var normalizedAnswer = answerDto.Answer.ToUpper().Trim();

            if (existingAnswers.TryGetValue(answerDto.QuestionSeq, out var existing))
            {
                existing.UpdateAnswer(normalizedAnswer);
            }
            else
            {
                var quizAnswer = QuizAnswer.Create(userId, dailyReadId, answerDto.QuestionSeq, normalizedAnswer);
                await dbContext.AddAsync(quizAnswer, ct);
            }
        }

        var result = await unitOfWork.SaveChangesAsync(ct);

        if (result.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
            return;
        }

        await Send.OkAsync(Result.Success(), cancellation: ct);
    }
}
