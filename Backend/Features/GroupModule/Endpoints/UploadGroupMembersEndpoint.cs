using ClosedXML.Excel;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.GroupModule.Domain;

namespace PureTCOWebApp.Features.GroupModule.Endpoints;

public class UploadGroupMembersRequest
{
    public int Id { get; set; }
    public IFormFile? File { get; set; }
}

public record UploadGroupMembersResponse(
    int ImportedCount,
    List<string> UnmatchedNims,
    List<string> MovedSummary);

public class UploadGroupMembersEndpoint(
    ApplicationDbContext dbContext,
    UnitOfWork unitOfWork
) : Endpoint<UploadGroupMembersRequest, ApiResponse<UploadGroupMembersResponse>>
{
    public override void Configure()
    {
        Post("{id}/members/upload");
        Group<GroupEndpointGroup>();
        Roles("admin");
        AllowFileUploads();
    }

    public override async Task HandleAsync(UploadGroupMembersRequest req, CancellationToken ct)
    {
        var group = await dbContext.Groups.FindAsync([req.Id], ct);

        if (group is null)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(
                (Result)CrudDomainError.NotFound("Group", req.Id)));
            return;
        }

        if (req.File is null || req.File.Length == 0)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(
                (Result)new Error("FileRequired", "An Excel file is required.")));
            return;
        }

        // Parse Excel
        List<string> nims;
        try
        {
            nims = ParseTemplateB(req.File);
        }
        catch (Exception ex)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(
                (Result)new Error("InvalidFile", $"Failed to parse Excel file: {ex.Message}")));
            return;
        }

        if (nims.Count == 0)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(
                (Result)new Error("EmptyFile", "No NIM rows found in the uploaded file.")));
            return;
        }

        nims = nims.Distinct().ToList();

        // Batch-load all users by NIM
        var usersByNim = await dbContext.Users
            .Where(u => nims.Contains(u.Nim))
            .ToDictionaryAsync(u => u.Nim, ct);

        // Full replace: delete all existing members of this group
        var existingMembers = await dbContext.GroupMembers
            .Where(m => m.GroupId == req.Id)
            .ToListAsync(ct);
        dbContext.GroupMembers.RemoveRange(existingMembers);

        var unmatchedNims = new List<string>();
        var movedSummary = new List<string>();
        int importedCount = 0;

        foreach (var nim in nims)
        {
            if (!usersByNim.TryGetValue(nim, out var user))
            {
                unmatchedNims.Add(nim);
                continue;
            }

            // Remove user from any other group if needed
            var otherMembership = await dbContext.GroupMembers
                .Include(m => m.Group)
                .FirstOrDefaultAsync(m => m.UserId == user.Id && m.GroupId != req.Id, ct);

            if (otherMembership is not null)
            {
                movedSummary.Add($"{user.Nim} moved from '{otherMembership.Group.Name}' to group #{req.Id}");
                dbContext.GroupMembers.Remove(otherMembership);
            }

            var member = GroupMember.Create(req.Id, user.Id);
            await dbContext.GroupMembers.AddAsync(member, ct);
            importedCount++;
        }

        var result = await unitOfWork.SaveChangesAsync(ct);

        if (result.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
            return;
        }

        await Send.OkAsync(Result.Success(
            new UploadGroupMembersResponse(importedCount, unmatchedNims, movedSummary)
        ), cancellation: ct);
    }

    private static List<string> ParseTemplateB(IFormFile file)
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

        if (!headers.ContainsKey("NIM"))
            throw new InvalidOperationException("Excel must contain a 'NIM' column.");

        var nimCol = headers["NIM"];
        var nims = new List<string>();
        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

        for (int r = 2; r <= lastRow; r++)
        {
            var nim = ws.Cell(r, nimCol).GetString().Trim();
            if (!string.IsNullOrEmpty(nim))
                nims.Add(nim);
        }

        return nims;
    }
}
