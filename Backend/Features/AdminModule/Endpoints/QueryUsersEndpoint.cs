using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using PureTCOWebApp.Core.Paging;
using PureTCOWebApp.Features.Auth.Domain;

namespace PureTCOWebApp.Features.AdminModule.Endpoints;

public record QueryUsersRequest(
    string? Email = null,
    string? UserName = null,
    string? Nim = null,
    string? ProgramStudy = null,
    List<string>? Roles = null
) : PagingQuery;

public record QueryUsersResponse(
    int Id,
    string? UserName,
    string? Email,
    string? Nim,
    string? ProgramStudy,
    string? Faculty,
    string? GenerationYear,
    List<string> Roles,
    bool Disabled
);

public class QueryUsersEndpoint(UserManager<User> userManager)
    : Endpoint<QueryUsersRequest, PagingResult<QueryUsersResponse>>
{
    public override void Configure()
    {
        Get("");
        Group<AdminEndpointGroup>();
    }

    public override async Task HandleAsync(QueryUsersRequest req, CancellationToken ct)
    {
        var query = userManager.Users.AsQueryable();

        if (!string.IsNullOrEmpty(req.Search))
        {
            var searchLower = req.Search.ToLower();
            query = query.Where(u =>
                u.UserName!.ToLower().Contains(searchLower) ||
                u.Email!.ToLower().Contains(searchLower) ||
                u.Nim.ToLower().Contains(searchLower) ||
                u.ProgramStudy.ToLower().Contains(searchLower) ||
                u.Faculty.ToLower().Contains(searchLower) ||
                u.GenerationYear.ToLower().Contains(searchLower)
            );
        }

        if (req.Roles != null && req.Roles.Count > 0)
        {
            var userIds = new List<int>();
            foreach (var role in req.Roles)
            {
                var usersInRole = await userManager.GetUsersInRoleAsync(role);
                userIds.AddRange(usersInRole.Select(u => u.Id));
            }
            query = query.Where(u => userIds.Contains(u.Id));
        }

        var result = await PagingService.PaginateQueryAsync(query, req, user =>
        {
            var roles = userManager.GetRolesAsync(user).Result;
            return new QueryUsersResponse(
                user.Id,
                user.UserName,
                user.Email,
                user.Nim,
                user.ProgramStudy,
                user.Faculty,
                user.GenerationYear,
                [.. roles],
                user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow
            );
        }, ct);

        await Send.OkAsync(result, ct);
    }
}
