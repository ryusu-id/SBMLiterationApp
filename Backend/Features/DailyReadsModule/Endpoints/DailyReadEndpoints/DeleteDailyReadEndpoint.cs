using FastEndpoints;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.DailyReadsModule.Endpoints.DailyReadEndpoints;

public class DeleteDailyReadEndpoint(
    ApplicationDbContext dbContext,
    UnitOfWork unitOfWork
) : EndpointWithoutRequest<ApiResponse>
{
    public override void Configure()
    {
        Delete("{id}");
        Group<DailyReadsEndpointGroup>();
        Roles("admin");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");
        var dailyRead = await dbContext.DailyReads.FindAsync([id], ct);

        if (dailyRead == null)
        {
            await Send.ResultAsync(TypedResults.NotFound<ApiResponse>((Result)CrudDomainError.NotFound("DailyRead", id)));
            return;
        }

        dbContext.DailyReads.Remove(dailyRead);
        var result = await unitOfWork.SaveChangesAsync(ct);

        if (result.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
            return;
        }

        await Send.OkAsync(Result.Success(), cancellation: ct);
    }
}
