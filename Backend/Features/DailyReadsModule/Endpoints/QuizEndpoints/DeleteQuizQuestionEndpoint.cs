using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.DailyReadsModule.Endpoints.QuizEndpoints;

public class DeleteQuizQuestionEndpoint(
    ApplicationDbContext dbContext,
    UnitOfWork unitOfWork
) : EndpointWithoutRequest<ApiResponse>
{
    public override void Configure()
    {
        Delete("{dailyReadId}/quiz/{questionSeq}");
        Group<DailyReadsEndpointGroup>();
        Roles("admin");
    }

    public override async Task HandleAsync(CancellationToken ct)
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

        dbContext.Remove(quizQuestion);
        var result = await unitOfWork.SaveChangesAsync(ct);

        if (result.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
            return;
        }

        await Send.OkAsync(Result.Success(), ct);
    }
}
