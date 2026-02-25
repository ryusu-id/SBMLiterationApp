using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core.Paging;
using PureTCOWebApp.Data;
using DomainGroup = PureTCOWebApp.Features.GroupModule.Domain.Group;

namespace PureTCOWebApp.Features.GroupModule.Endpoints;

public record QueryGroupsRequest(string? Name = null) : PagingQuery;

public record GroupListItem(int Id, string Name, string? Description, int MemberCount, DateTime CreateTime);

public class QueryGroupsEndpoint(ApplicationDbContext dbContext)
    : Endpoint<QueryGroupsRequest, PagingResult<GroupListItem>>
{
    public override void Configure()
    {
        Get("");
        Group<GroupEndpointGroup>();
        Roles("admin");
    }

    public override async Task HandleAsync(QueryGroupsRequest req, CancellationToken ct)
    {
        // Sort on the entity query (DomainGroup.CreateTime) to avoid EF Core translation
        // issues when sorting a projected record constructor type.
        var query = dbContext.Groups
            .Include(g => g.Members)  // loaded so .Count works after materialization
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.Name))
        {
            query = query.Where(g => g.Name.Contains(req.Name));
        }

        // PaginateQueryAsync<DomainGroup, GroupListItem> sorts on the entity, then maps
        var result = await PagingService.PaginateQueryAsync<DomainGroup, GroupListItem>(
            query,
            req,
            g => new GroupListItem(g.Id, g.Name, g.Description, g.Members.Count, g.CreateTime),
            ct);

        await Send.OkAsync(result, ct);
    }
}
