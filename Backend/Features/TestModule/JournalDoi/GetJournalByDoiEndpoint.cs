using FastEndpoints;
using PureTCOWebApp.Core;

namespace PureTCOWebApp.Features.TestModule.JournalDoi;

public class GetJournalByDoiRequest
{
    public string Doi { get; set; } = string.Empty;
}

public class GetJournalByDoiEndpoint : Endpoint<GetJournalByDoiRequest, ApiResponse<WorkMessage>>
{
    private readonly CrossRefService _crossRefService;

    public GetJournalByDoiEndpoint(CrossRefService crossRefService)
    {
        _crossRefService = crossRefService;
    }

    public override void Configure()
    {
        Get("/journal-doi");
        Group<TestModuleEndpointGroup>();
        AllowAnonymous();
        Summary(s => s.Summary = "Get journal information by DOI from CrossRef API");
    }

    public override async Task HandleAsync(GetJournalByDoiRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Doi))
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var result = await _crossRefService.GetWorkByDoiAsync(req.Doi);
        
        if (result == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var response = new ApiResponse<WorkMessage>("Success", result, null, null);
        await Send.OkAsync(response, ct);
    }
}
