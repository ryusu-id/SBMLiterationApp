using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Core.Upload;
using PureTCOWebApp.Core.Upload.Attributes;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.GroupModule.Domain;

namespace PureTCOWebApp.Features.GroupModule.Endpoints;

public class GroupMasterUploadDtoValidator : AbstractValidator<GroupMasterUploadDto>
{
    public GroupMasterUploadDtoValidator()
    {
        RuleFor(x => x.GroupName)
            .NotEmpty().WithMessage("Group Name is required.");

        RuleFor(x => x.Nim)
            .NotEmpty().WithMessage("NIM is required.");
    }
}

public class GroupMasterUploadDto
{
    [ExcelColumn("A", "GroupName")] public string? GroupName { get; set; }
    [ExcelColumn("B", "NIM")] public string? Nim { get; set; }
}

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
        if (req.File is null)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(
                (Result)new Error("FileRequired", "An Excel file is required.")));
            return;
        }

        var validateResult = ExcelHelper.ValidateExcelFile(req.File);
        if (validateResult.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(validateResult));
            return;
        }

        var fileBytes = new byte[req.File.Length];
        using (var stream = req.File.OpenReadStream())
        {
            await stream.ReadExactlyAsync(fileBytes, 0, (int)req.File.Length, ct);
        }

        var structureResult = ExcelHelper.ValidateColumnStructure<GroupMasterUploadDto>(fileBytes);
        if (structureResult.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(structureResult));
            return;
        }

        List<GroupMasterUploadDto> parsedRows;
        try
        {
            parsedRows = ExcelHelper.ParseExcelData<GroupMasterUploadDto>(fileBytes);
        }
        catch (Exception ex)
        {
            var parseError = BulkResult.Failure(ExcelDomainError.OneOrMoreValidationError)
                .SetErrors([new Error("ExcelParseError", $"Invalid Excel File: {ex.Message}")]);
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(parseError));
            return;
        }

        var validationResult = ValidateRows(parsedRows);
        if (validationResult.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(validationResult));
            return;
        }

        var rows = parsedRows
            .Where(r => !string.IsNullOrWhiteSpace(r.GroupName) && !string.IsNullOrWhiteSpace(r.Nim))
            .Select(r => (GroupName: r.GroupName!.Trim(), Nim: r.Nim!.Trim()))
            .ToList();

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

    private static BulkResult ValidateRows(ICollection<GroupMasterUploadDto> rows)
    {
        ICollection<Result> results = [];
        var idx = 2;
        var validator = new GroupMasterUploadDtoValidator();

        foreach (var row in rows)
        {
            var validation = validator.Validate(row);

            foreach (var failure in validation.Errors)
            {
                var cell = ResolveExcelColumn(failure.PropertyName, idx);
                results.Add(ExcelDomainError.ValidationError(cell, failure.ErrorMessage));
            }

            idx++;
        }

        if (results.Any(r => r.IsFailure))
        {
            return BulkResult
                .Failure(ExcelDomainError.OneOrMoreValidationError)
                .SetErrors([.. results.Where(e => e.IsFailure).Select(e => e.Error)]);
        }

        return BulkResult.Success();
    }

    private static string ResolveExcelColumn(string? propertyName, int rowIndex)
    {
        var prop = propertyName?.ToLowerInvariant() ?? "";

        if (prop.Contains("groupname")) return $"A{rowIndex}";
        if (prop.Contains("nim")) return $"B{rowIndex}";

        return $"Unknown{rowIndex}";
    }

}
