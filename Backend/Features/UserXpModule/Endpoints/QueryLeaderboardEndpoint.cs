using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core.Paging;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.UserXpModule;
using PureTCOWebApp.Features.UserXpModuleModule.Endpoints;
using static PureTCOWebApp.Features.UserXpModule.UserExpDomainService;

namespace PureTCOWebApp.Features.UserXpModuleModule.Endpoints;

public record QueryLeaderboardRequest(string? CategoryName = null) : PagingQuery;

public class QueryLeaderboardEndpoint(UserExpDomainService userExpDomainService)
    : Endpoint<QueryLeaderboardRequest, PagingResult<UserExp>>
{
    public override void Configure()
    {
        Get("leaderboard");
        Group<GlobalApiEndpointGroup>();
    }

    public override async Task HandleAsync(QueryLeaderboardRequest req, CancellationToken ct)
    {
        var result = await userExpDomainService.QueryLeaderboard(req, ct);

        await Send.OkAsync(result, ct);
    }
}