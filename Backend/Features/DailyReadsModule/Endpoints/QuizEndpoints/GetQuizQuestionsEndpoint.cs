using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.DailyReadsModule.Endpoints.QuizEndpoints;

public record QuizQuestionResponse(
    int Id,
    int QuestionSeq,
    string Question,
    string CorrectAnswer,
    List<QuizChoiceResponse> Choices
);

public record QuizChoiceResponse(
    int Id,
    string Choice,
    string Answer
);

public class GetQuizQuestionsEndpoint(ApplicationDbContext dbContext)
    : EndpointWithoutRequest<ApiResponse<List<QuizQuestionResponse>>>
{
    public override void Configure()
    {
        Get("{dailyReadId}/quiz");
        Group<DailyReadsEndpointGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var dailyReadId = Route<int>("dailyReadId");

        var questions = await dbContext.QuizQuestions
            .Include(q => q.Choices)
            .Where(q => q.DailyReadId == dailyReadId)
            .OrderBy(q => q.QuestionSeq)
            .Select(q => new QuizQuestionResponse(
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
