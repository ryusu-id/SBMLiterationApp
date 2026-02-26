using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core.Paging;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.ReadingResourceModule.Domain;

namespace PureTCOWebApp.Features.ReadingResourceModule.Endpoints.BookEndpoints;

public record BookWithProgress(
    int Id,
    int UserId,
    string Title,
    string ISBN,
    string? ReadingCategory,
    string Authors,
    string PublishYear,
    int Page,
    string? ResourceLink,
    string? CoverImageUri,
    string CssClass,
    int? LastReadPage
);

public record QueryBookRequest(
    string? Title = null,
    string? ISBN = null,
    string? ReadingCategory = null,
    string? Authors = null,
    string? PublishYear = null
) : PagingQuery;

public class QueryBookEndpoint(ApplicationDbContext dbContext)
    : Endpoint<QueryBookRequest, PagingResult<BookWithProgress>>
{
    public override void Configure()
    {
        Get("books");
        Group<ReadingResourceEndpointGroup>();
    }

    public override async Task HandleAsync(QueryBookRequest req, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        
        var query = dbContext.Books.Where(x => x.UserId == userId);

        var predicate = PredicateBuilder.True<Book>();

        if (!string.IsNullOrWhiteSpace(req.Title))
        {
            predicate = predicate.And(x => x.Title != null && x.Title.Contains(req.Title));
        }

        if (!string.IsNullOrWhiteSpace(req.ISBN))
        {
            predicate = predicate.And(x => x.ISBN != null && x.ISBN.Contains(req.ISBN));
        }

        if (!string.IsNullOrWhiteSpace(req.ReadingCategory))
        {
            predicate = predicate.And(x => x.ReadingCategory != null && x.ReadingCategory.Contains(req.ReadingCategory));
        }

        if (!string.IsNullOrWhiteSpace(req.Authors))
        {
            predicate = predicate.And(x => x.Authors != null && x.Authors.Contains(req.Authors));
        }

        if (!string.IsNullOrWhiteSpace(req.PublishYear))
        {
            predicate = predicate.And(x => x.PublishYear != null && x.PublishYear.Contains(req.PublishYear));
        }

        query = query.Where(predicate);

        var queryWithLastRead = from book in query
                                select new
                                {
                                    Book = book,
                                    LastReadDate = dbContext.ReadingReports
                                        .Where(r => r.ReadingResourceId == book.Id)
                                        .OrderByDescending(r => r.ReportDate)
                                        .Select(r => (DateTime?)r.ReportDate)
                                        .FirstOrDefault(),
                                    LastReadPage = dbContext.ReadingReports
                                        .Where(r => r.ReadingResourceId == book.Id)
                                        .OrderByDescending(r => r.ReportDate)
                                        .Select(r => (int?)r.CurrentPage)
                                        .FirstOrDefault()
                                };

        queryWithLastRead = queryWithLastRead.OrderByDescending(x => x.LastReadDate);

        // Override SortBy to prevent PagingService from re-ordering
        req = req with { SortBy = "" };

        var pagedBooks = await PagingService.PaginateQueryAsync(queryWithLastRead.Select(x => x.Book), req, dbContext, ct);

        var bookIds = pagedBooks.Rows.Select(b => b.Id).ToList();
        var lastReadData = await queryWithLastRead
            .Where(x => bookIds.Contains(x.Book.Id))
            .ToDictionaryAsync(x => x.Book.Id, x => x.LastReadPage, ct);

        var booksWithProgress = pagedBooks.Rows.Select(b => new BookWithProgress(
            b.Id,
            b.UserId,
            b.Title,
            b.ISBN,
            b.ReadingCategory,
            b.Authors,
            b.PublishYear,
            b.Page,
            b.ResourceLink,
            b.CoverImageUri,
            b.CssClass,
            lastReadData.GetValueOrDefault(b.Id)
        )).ToList();

        var result = new PagingResult<BookWithProgress>(
            booksWithProgress,
            pagedBooks.Page,
            pagedBooks.RowsPerPage,
            pagedBooks.TotalRows,
            pagedBooks.TotalPages,
            pagedBooks.SearchText,
            pagedBooks.SortBy,
            pagedBooks.SortDirection
        );

        await Send.OkAsync(result, cancellation: ct);
    }
}