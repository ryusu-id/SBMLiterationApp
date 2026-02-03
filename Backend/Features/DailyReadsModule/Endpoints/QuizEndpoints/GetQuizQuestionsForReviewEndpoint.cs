using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.DailyReadsModule.Endpoints.QuizEndpoints;

public record QuizChoiceResponse(
    int Id,
    string Choice,
    string Answer
);

public record QuizQuestionForReviewResponse(
    int Id,
    int QuestionSeq,
    string Question,
    string CorrectAnswer,
    List<QuizChoiceResponse> Choices
);

public class GetQuizQuestionsForReviewEndpoint(ApplicationDbContext dbContext)
    : EndpointWithoutRequest<ApiResponse<List<QuizQuestionForReviewResponse>>>
{
    public override void Configure()
    {
        Get("{dailyReadId}/quiz/review");
        Group<DailyReadsEndpointGroup>();
        Roles("admin");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var dailyReadId = Route<int>("dailyReadId");

        var questions = await dbContext.QuizQuestions
            .Include(q => q.Choices)
            .Where(q => q.DailyReadId == dailyReadId)
            .OrderBy(q => q.QuestionSeq)
            .Select(q => new QuizQuestionForReviewResponse(
                q.Id,
                q.QuestionSeq,
                q.Question,
                q.CorrectAnswer,
                q.Choices.Select(c => new QuizChoiceResponse(c.Id, c.Choice, c.Answer)).ToList()
            ))
            .ToListAsync(ct);

        await Send.OkAsync(Result.Success(questions), cancellation: ct);
    }
}
