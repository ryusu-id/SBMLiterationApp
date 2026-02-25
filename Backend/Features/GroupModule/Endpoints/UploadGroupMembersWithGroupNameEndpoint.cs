using ClosedXML.Excel;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.GroupModule.Domain;

namespace PureTCOWebApp.Features.GroupModule.Endpoints;

public class UploadGroupMembersWithGroupNameRequest
{
    public IFormFile? File { get; set; }
}

public record UploadGroupMembersWithGroupNameResponse(
    int ImportedCount,
    List<string> UnmatchedNims,
    List<string> MovedSummary);

public class UploadGroupMembersWithGroupNameEndpoint(
    ApplicationDbContext dbContext,
    UnitOfWork unitOfWork
) : Endpoint<UploadGroupMembersWithGroupNameRequest, ApiResponse<UploadGroupMembersWithGroupNameResponse>>
{
    public override void Configure()
    {
        Post("members/upload");
        Group<GroupEndpointGroup>();
        Roles("admin");
        AllowFileUploads();
    }

    public override async Task HandleAsync(UploadGroupMembersWithGroupNameRequest req, CancellationToken ct)
    {
        if (req.File is null || req.File.Length == 0)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(
                (Result)new Error("FileRequired", "An Excel file is required.")));
            return;
        }

        // Parse Excel
        List<(string GroupName, string Nim)> rows;
        try
        {
            rows = ParseTemplateA(req.File);
        }
        catch (Exception ex)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(
                (Result)new Error("InvalidFile", $"Failed to parse Excel file: {ex.Message}")));
            return;
        }

        if (rows.Count == 0)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(
                (Result)new Error("EmptyFile", "No data rows found in the uploaded file.")));
            return;
        }

        // Group rows by group name
        var rowsByGroupName = rows
            .GroupBy(r => r.GroupName.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Select(r => r.Nim.Trim()).Distinct().ToList());

        // Load existing groups matching mentioned names
        var groupNames = rowsByGroupName.Keys.ToList();
        var existingGroups = await dbContext.Groups
            .Where(g => groupNames.Contains(g.Name))
            .ToDictionaryAsync(g => g.Name, StringComparer.OrdinalIgnoreCase, ct);

        // Find or create each group entity, track all
        var groupEntities = new Dictionary<string, Domain.Group>(StringComparer.OrdinalIgnoreCase);
        foreach (var groupName in rowsByGroupName.Keys)
        {
            if (!existingGroups.TryGetValue(groupName, out var g))
            {
                g = Domain.Group.Create(groupName, null);
                await dbContext.Groups.AddAsync(g, ct);
            }
            groupEntities[groupName] = g;
        }

        // Batch delete existing members of affected groups (full replace)
        var affectedGroupIds = existingGroups.Values.Select(g => g.Id).ToList();
        if (affectedGroupIds.Count > 0)
        {
            var membersToRemove = await dbContext.GroupMembers
                .Where(m => affectedGroupIds.Contains(m.GroupId))
                .ToListAsync(ct);
            dbContext.GroupMembers.RemoveRange(membersToRemove);
        }

        // Phase 1 save: persist new groups (gets real IDs) and removes old members
        var phase1Result = await unitOfWork.SaveChangesAsync(ct);
        if (phase1Result.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(phase1Result));
            return;
        }

        // Batch-load all users by NIM
        var allNims = rows.Select(r => r.Nim.Trim()).Distinct().ToList();
        var usersByNim = await dbContext.Users
            .Where(u => allNims.Contains(u.Nim))
            .ToDictionaryAsync(u => u.Nim, ct);

        // Load all found user IDs to detect cross-group moves
        var foundUserIds = usersByNim.Values.Select(u => u.Id).ToList();
        var allNewGroupIds = groupEntities.Values.Select(g => g.Id).ToList();

        // Find users who still have membership in OTHER groups not touched by this upload
        var crossGroupMembers = await dbContext.GroupMembers
            .Include(m => m.Group)
            .Where(m => foundUserIds.Contains(m.UserId) && !allNewGroupIds.Contains(m.GroupId))
            .ToListAsync(ct);

        var movedSummary = crossGroupMembers
            .Select(m => $"{usersByNim.Values.First(u => u.Id == m.UserId).Nim} moved from '{m.Group.Name}'")
            .ToList();

        dbContext.GroupMembers.RemoveRange(crossGroupMembers);

        // Phase 2: insert new members
        var unmatchedNims = new List<string>();
        int importedCount = 0;

        foreach (var (groupName, nims) in rowsByGroupName)
        {
            var group = groupEntities[groupName];

            foreach (var nim in nims)
            {
                if (!usersByNim.TryGetValue(nim, out var user))
                {
                    unmatchedNims.Add(nim);
                    continue;
                }

                var member = GroupMember.Create(group.Id, user.Id);
                await dbContext.GroupMembers.AddAsync(member, ct);
                importedCount++;
            }
        }

        var result = await unitOfWork.SaveChangesAsync(ct);

        if (result.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
            return;
        }

        await Send.OkAsync(Result.Success(
            new UploadGroupMembersWithGroupNameResponse(importedCount, unmatchedNims, movedSummary)
        ), cancellation: ct);
    }

    private static List<(string GroupName, string Nim)> ParseTemplateA(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        using var workbook = new XLWorkbook(stream);
        var ws = workbook.Worksheets.First();

        var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var headerRow = ws.Row(1);
        foreach (var cell in headerRow.CellsUsed())
        {
            headers[cell.GetString().Trim()] = cell.Address.ColumnNumber;
        }

        if (!headers.ContainsKey("GroupName") || !headers.ContainsKey("NIM"))
            throw new InvalidOperationException("Excel must contain 'GroupName' and 'NIM' columns.");

        var groupNameCol = headers["GroupName"];
        var nimCol = headers["NIM"];

        var rows = new List<(string, string)>();
        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

        for (int r = 2; r <= lastRow; r++)
        {
            var groupName = ws.Cell(r, groupNameCol).GetString().Trim();
            var nim = ws.Cell(r, nimCol).GetString().Trim();

            if (string.IsNullOrEmpty(groupName) || string.IsNullOrEmpty(nim)) continue;

            rows.Add((groupName, nim));
        }

        return rows;
    }
}
