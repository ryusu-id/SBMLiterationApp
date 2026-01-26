using FastEndpoints;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;

namespace PureTCOWebApp.Features.IntegrationModule.CrossRef.Endpoints;

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
        Group<GlobalApiEndpointGroup>();
        Summary(s => s.Summary = "Get journal information by DOI from CrossRef API");
    }

    public override async Task HandleAsync(GetJournalByDoiRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Doi))
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        try
        {
            var result = await _crossRefService.GetWorkByDoiAsync(req.Doi);
            if (result == null)
            {
                await Send.NotFoundAsync(ct);
                return;
            }
            var response = new ApiResponse<WorkMessage>("Success", result, null, null);
            await Send.OkAsync(response, ct);
        }
        catch
        {
            await Send.ResultAsync(TypedResults.NotFound<ApiResponse>((Result)new Error("DoiNotFound", "DOI not found")));
        }
    }
}
