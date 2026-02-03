using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.DailyReadsModule.Endpoints.DailyReadEndpoints;

public class GetDailyReadByIdForParticipantEndpoint(ApplicationDbContext dbContext)
    : EndpointWithoutRequest<ApiResponse<DailyReadWithQuizResult>>
{
    public override void Configure()
    {
        Get("{id}/participant");
        Group<DailyReadsEndpointGroup>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");
        var userId = int.Parse(User.FindFirst("sub")!.Value);

        var dailyRead = await dbContext.DailyReads.FindAsync([id], ct);

        var today = DateOnly.FromDateTime(DateTime.UtcNow.ToLocalTime());
        if (dailyRead == null || dailyRead.Date > today)
        {
            await Send.ResultAsync(TypedResults.NotFound<ApiResponse>((Result)CrudDomainError.NotFound("DailyRead", id)));
            return;
        }

        var questions = await dbContext.QuizQuestions
            .Where(q => q.DailyReadId == id)
            .ToListAsync(ct);

        var userAnswers = await dbContext.QuizAnswers
            .Where(a => a.UserId == userId && a.DailyReadId == id)
            .GroupBy(a => a.QuestionSeq)
            .Select(g => g.OrderByDescending(a => a.RetrySeq).First())
            .ToListAsync(ct);

        QuizResultInfo? quizResult = null;

        if (questions.Any())
        {
            var correctCount = userAnswers.Count(a =>
                questions.Any(q => q.QuestionSeq == a.QuestionSeq &&
                                   q.CorrectAnswer.Equals(a.Answer, StringComparison.OrdinalIgnoreCase)));

            quizResult = new QuizResultInfo(
                questions.Count,
                correctCount,
                userAnswers.Any(),
                correctCount >= dailyRead.MinimalCorrectAnswer
            );
        }

        var response = new DailyReadWithQuizResult(
            dailyRead.Id,
            dailyRead.Title,
            dailyRead.CoverImg,
            dailyRead.Content,
            dailyRead.Date,
            dailyRead.Category,
            dailyRead.Exp,
            dailyRead.MinimalCorrectAnswer,
            quizResult
        );

        await Send.OkAsync(Result.Success(response), cancellation: ct);
    }
}
