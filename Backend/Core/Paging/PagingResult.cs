using PureTCOWebApp.Core.Models;

namespace PureTCOWebApp.Core.Paging;

public record PagingResult<T>(
    IEnumerable<T> Rows,
    int Page,
    int RowsPerPage,
    int TotalRows,
    int TotalPages,
    string SearchText = "",
    string SortBy = "",
    string SortDirection = ""
)
{
    public static implicit operator Result<PagingResult<T>>(PagingResult<T> pagingResult) => Result.Success(pagingResult);
};
