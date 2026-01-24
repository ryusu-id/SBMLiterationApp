using FastEndpoints;
using PureTCOWebApp.Core;

namespace PureTCOWebApp.Features.TestModule.GoogleBook;

public class GetBookByIdRequest
{
    public string VolumeId { get; set; } = string.Empty;
}

public class GetBookByIdEndpoint : Endpoint<GetBookByIdRequest, ApiResponse<BookItem>>
{
    private readonly GoogleBooksService _googleBooksService;

    public GetBookByIdEndpoint(GoogleBooksService googleBooksService)
    {
        _googleBooksService = googleBooksService;
    }

    public override void Configure()
    {
        Get("/google-books/{VolumeId}");
        Group<TestModuleEndpointGroup>();
        AllowAnonymous();
        Summary(s => s.Summary = "Get book details by volume ID");
    }

    public override async Task HandleAsync(GetBookByIdRequest req, CancellationToken ct)
    {
        var result = await _googleBooksService.GetBookByIdAsync(req.VolumeId);
        
        if (result == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var response = new ApiResponse<BookItem>("Success", result, null, null);
        await Send.OkAsync(response, ct);
    }
}
