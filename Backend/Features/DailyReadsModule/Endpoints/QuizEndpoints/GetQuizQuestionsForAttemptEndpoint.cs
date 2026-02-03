using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.DailyReadsModule.Endpoints.QuizEndpoints;

public record QuizQuestionForAttemptResponse(
    int QuestionSeq,
    string Question,
    List<QuizChoiceResponse> Choices
);

public class GetQuizQuestionsForAttemptEndpoint(ApplicationDbContext dbContext)
    : EndpointWithoutRequest<ApiResponse<List<QuizQuestionForAttemptResponse>>>
{
    public override void Configure()
    {
        Get("{dailyReadId}/quiz/attempt");
        Group<DailyReadsEndpointGroup>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var dailyReadId = Route<int>("dailyReadId");
        var userId = int.Parse(User.FindFirst("sub")!.Value);

        var dailyRead = await dbContext.DailyReads.FindAsync([dailyReadId], ct);
        if (dailyRead == null)
        {
            await Send.ResultAsync(TypedResults.NotFound<ApiResponse>((Result)CrudDomainError.NotFound("DailyRead", dailyReadId)));
            return;
        }

        if (dailyRead.Date != DateOnly.FromDateTime(DateTime.UtcNow.ToLocalTime()))
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(
                Result.Failure(new Error("Quiz.NotAvailable", "Quiz is not available for this daily read"))
            ));
            return;
        }

        var latestAnswers = await dbContext.QuizAnswers
            .Where(a => a.UserId == userId && a.DailyReadId == dailyReadId)
            .GroupBy(a => a.QuestionSeq)
            .Select(g => g.OrderByDescending(a => a.RetrySeq).First())
            .ToListAsync(ct);

        if (latestAnswers.Any())
        {
            var questions = await dbContext.QuizQuestions
                .Where(q => q.DailyReadId == dailyReadId)
                .ToListAsync(ct);

            var correctCount = latestAnswers.Count(a =>
                questions.Any(q => q.QuestionSeq == a.QuestionSeq &&
                                   q.CorrectAnswer.Equals(a.Answer, StringComparison.OrdinalIgnoreCase)));

            if (correctCount >= dailyRead.MinimalCorrectAnswer)
            {
                await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(
                    Result.Failure(new Error("Quiz.AlreadyPassed", "You have already passed this quiz"))
                ));
                return;
            }
        }

        var quizQuestions = await dbContext.QuizQuestions
            .Include(q => q.Choices)
            .Where(q => q.DailyReadId == dailyReadId)
            .OrderBy(q => q.QuestionSeq)
            .Select(q => new QuizQuestionForAttemptResponse(
                q.QuestionSeq,
                q.Question,
                q.Choices.Select(c => new QuizChoiceResponse(c.Id, c.Choice, c.Answer)).ToList()
            ))
            .ToListAsync(ct);

        await Send.OkAsync(Result.Success(quizQuestions), cancellation: ct);
    }
}
