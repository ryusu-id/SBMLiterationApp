using FastEndpoints;
using PureTCOWebApp.Core.Paging;

namespace PureTCOWebApp.Features.TestModule.GoogleBook;

public record SearchBooksRequest(string Query = "") : PagingQuery;

public class SearchBooksEndpoint : Endpoint<SearchBooksRequest, PagingResult<BookItem>>
{
    private readonly GoogleBooksService _googleBooksService;

    public SearchBooksEndpoint(GoogleBooksService googleBooksService)
    {
        _googleBooksService = googleBooksService;
    }

    public override void Configure()
    {
        Get("/google-books/search");
        Summary(s => s.Summary = "Search books using Google Books API with pagination");
        Group<GlobalApiEndpointGroup>();
    }

    public override async Task HandleAsync(SearchBooksRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Query))
        {
            await Send.OkAsync(new PagingResult<BookItem>([], req.Page, req.RowsPerPage, 0, 0, ""), ct);
            return;
        }

        var startIndex = (req.Page - 1) * req.RowsPerPage;
        var result = await _googleBooksService.SearchBooksAsync(req.Query, req.RowsPerPage, startIndex);
        
        if (result == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var totalPages = (int)Math.Ceiling((double)result.TotalItems / req.RowsPerPage);
        
        var pagingResult = new PagingResult<BookItem>(
            Rows: result.Items ?? new List<BookItem>(),
            Page: req.Page,
            RowsPerPage: req.RowsPerPage,
            TotalRows: result.TotalItems,
            TotalPages: totalPages,
            SearchText: req.Query
        );

        await Send.OkAsync(pagingResult, ct);
    }
}
