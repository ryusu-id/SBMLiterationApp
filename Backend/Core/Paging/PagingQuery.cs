namespace PureTCOWebApp.Core.Paging;

public record PagingQuery(
    int Page = 1,
    int RowsPerPage = 15,
    string? Search = null,
    string? SortBy = null,
    string? SortDescending = null
);
