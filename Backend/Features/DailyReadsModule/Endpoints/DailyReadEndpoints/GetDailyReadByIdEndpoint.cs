using FastEndpoints;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.DailyReadsModule.Domain;

namespace PureTCOWebApp.Features.DailyReadsModule.Endpoints.DailyReadEndpoints;

public class GetDailyReadByIdEndpoint(ApplicationDbContext dbContext)
    : EndpointWithoutRequest<ApiResponse<DailyRead>>
{
    public override void Configure()
    {
        Get("{id}");
        Group<DailyReadsEndpointGroup>();
        AllowAnonymous();
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

        await Send.OkAsync(Result.Success(dailyRead), cancellation: ct);
    }
}
