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
    string? Faculty = null,
    int? GenerationYear = null,
    List<string>? Roles = null
) : PagingQuery;

public record QueryUsersResponse(
    int Id,
    string? UserName,
    string? Email,
    string? Nim,
    string? ProgramStudy,
    string? Faculty,
    int? GenerationYear,
    List<string> Roles
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

        if (!string.IsNullOrWhiteSpace(req.Email))
            query = query.Where(u => u.Email!.Contains(req.Email));

        if (!string.IsNullOrWhiteSpace(req.UserName))
            query = query.Where(u => u.UserName!.Contains(req.UserName));

        if (!string.IsNullOrWhiteSpace(req.Nim))
            query = query.Where(u => u.Nim.Contains(req.Nim));

        if (!string.IsNullOrWhiteSpace(req.ProgramStudy))
            query = query.Where(u => u.ProgramStudy.Contains(req.ProgramStudy));

        if (!string.IsNullOrWhiteSpace(req.Faculty))
            query = query.Where(u => u.Faculty.Contains(req.Faculty));

        if (req.GenerationYear.HasValue)
            query = query.Where(u => u.GenerationYear == req.GenerationYear);

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
                [.. roles]
            );
        }, ct);

        await Send.OkAsync(result, ct);
    }
}
