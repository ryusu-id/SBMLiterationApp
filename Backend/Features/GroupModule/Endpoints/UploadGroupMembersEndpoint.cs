using ClosedXML.Excel;
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

public class GroupMembersUploadDtoValidator : AbstractValidator<GroupMembersUploadDto>
{
    public GroupMembersUploadDtoValidator()
    {
        RuleFor(x => x.Nim)
            .NotEmpty().WithMessage("NIM is required.");
    }
}

public class GroupMembersUploadDto
{
    [ExcelColumn("A", "NIM")] public string? Nim { get; set; }
}

public class UploadGroupMembersRequest
{
    public int Id { get; set; }
    public IFormFile? File { get; set; }
}

public record UploadGroupMembersResponse(
    int ImportedCount,
    List<string> UnmatchedNims);

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

        using (var ms = new MemoryStream(fileBytes))
        using (var wb = new XLWorkbook(ms))
        {
            var ws = wb.Worksheets.FirstOrDefault();
            var templateGroupName = ws?.Cell(1, 2).GetString().Trim() ?? string.Empty;
            if (!templateGroupName.Equals(group.Name, StringComparison.OrdinalIgnoreCase))
            {
                await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(
                    (Result)new Error("GroupName.Mismatch",
                        $"Template group name '{templateGroupName}' does not match the target group '{group.Name}'.")));
                return;
            }
        }

        var structureResult = ExcelHelper.ValidateColumnStructure<GroupMembersUploadDto>(fileBytes, hasHeaderRow: false);
        if (structureResult.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(structureResult));
            return;
        }

        List<GroupMembersUploadDto> rows;
        try
        {
            rows = ExcelHelper.ParseExcelData<GroupMembersUploadDto>(fileBytes, startRow: 3);
        }
        catch (Exception ex)
        {
            var parseError = BulkResult.Failure(ExcelDomainError.OneOrMoreValidationError)
                .SetErrors([new Error("ExcelParseError", $"Invalid Excel File: {ex.Message}")]);
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(parseError));
            return;
        }

        var validationResult = ValidateRows(rows);
        if (validationResult.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(validationResult));
            return;
        }

        var nims = rows
            .Where(r => !string.IsNullOrWhiteSpace(r.Nim))
            .Select(r => r.Nim!.Trim())
            .Distinct()
            .ToList();

        if (nims.Count == 0)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(
                (Result)new Error("EmptyFile", "No NIM rows found in the uploaded file.")));
            return;
        }

        var usersByNim = await dbContext.Users
            .Where(u => nims.Contains(u.Nim))
            .ToDictionaryAsync(u => u.Nim, ct);

        var foundUserIds = usersByNim.Values.Select(u => u.Id).ToList();
        var conflictingMemberships = await dbContext.GroupMembers
            .Include(m => m.Group)
            .Include(m => m.User)
            .Where(m => foundUserIds.Contains(m.UserId) && m.GroupId != req.Id)
            .ToListAsync(ct);

        if (conflictingMemberships.Count > 0)
        {
            var conflictErrors = conflictingMemberships
                .Select(m => new Error("Member.AlreadyInGroup",
                    $"The student {m.User.Fullname} with NIM {m.User.Nim} is already on group {m.Group.Name}."))
                .ToArray();

            var conflictResult = BulkResult
                .Failure(new Error("Upload.Conflicts", "One or more students are already assigned to another group."))
                .SetErrors(conflictErrors);

            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(conflictResult));
            return;
        }

        var existingMembers = await dbContext.GroupMembers
            .Where(m => m.GroupId == req.Id)
            .ToListAsync(ct);
        dbContext.GroupMembers.RemoveRange(existingMembers);

        var unmatchedNims = new List<string>();
        int importedCount = 0;

        foreach (var nim in nims)
        {
            if (!usersByNim.TryGetValue(nim, out var user))
            {
                unmatchedNims.Add(nim);
                continue;
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
            new UploadGroupMembersResponse(importedCount, unmatchedNims)
        ), cancellation: ct);
    }

    private static BulkResult ValidateRows(ICollection<GroupMembersUploadDto> rows)
    {
        ICollection<Result> results = [];
        var idx = 2;
        var validator = new GroupMembersUploadDtoValidator();

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

        if (prop.Contains("nim")) return $"A{rowIndex}";

        return $"Unknown{rowIndex}";
    }
}