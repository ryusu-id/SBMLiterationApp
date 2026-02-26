using ClosedXML.Excel;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.GroupModule.Endpoints;

public class DownloadGroupMembersTemplateRequest
{
    public int Id { get; set; }
}

public class DownloadGroupMembersTemplateEndpoint(ApplicationDbContext dbContext)
    : Endpoint<DownloadGroupMembersTemplateRequest>
{
    public override void Configure()
    {
        Get("{id}/members/template");
        Group<GroupEndpointGroup>();
        Roles("admin");
    }

    public override async Task HandleAsync(DownloadGroupMembersTemplateRequest req, CancellationToken ct)
    {
        var group = await dbContext.Groups.FindAsync([req.Id], ct);
        if (group is null)
        {
            await Send.ResultAsync(TypedResults.NotFound<ApiResponse>(
                (Result)CrudDomainError.NotFound("Group", req.Id)));
            return;
        }

        var members = await dbContext.GroupMembers
            .Where(m => m.GroupId == req.Id)
            .Include(m => m.User)
            .OrderBy(m => m.User.Nim)
            .Select(m => m.User.Nim)
            .ToListAsync(ct);

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Members");

        // Row 1: Group Name label + value
        sheet.Cell(1, 1).Value = "Group Name";
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 2).Value = group.Name;

        // Row 2: empty
        // Row 3: NIM column header
        sheet.Cell(3, 1).Value = "NIM";
        sheet.Cell(3, 1).Style.Font.Bold = true;

        // Data rows starting at row 4
        for (int i = 0; i < members.Count; i++)
        {
            sheet.Cell(i + 4, 1).Value = members[i];
        }

        sheet.Column(1).AdjustToContents();
        sheet.Column(2).AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var safeName = string.Concat(group.Name.Split(Path.GetInvalidFileNameChars()));
        await Send.StreamAsync(
            stream: stream,
            fileName: $"GroupMembers_{safeName}.xlsx",
            fileLengthBytes: stream.Length,
            contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            cancellation: ct
        );
    }
}
