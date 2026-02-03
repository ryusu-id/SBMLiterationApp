using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
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
    string ReadingCategory,
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
            predicate = predicate.And(x => x.Title.Contains(req.Title));
        }

        if (!string.IsNullOrWhiteSpace(req.ISBN))
        {
            predicate = predicate.And(x => x.ISBN.Contains(req.ISBN));
        }

        if (!string.IsNullOrWhiteSpace(req.ReadingCategory))
        {
            predicate = predicate.And(x => x.ReadingCategory.Contains(req.ReadingCategory));
        }

        if (!string.IsNullOrWhiteSpace(req.Authors))
        {
            predicate = predicate.And(x => x.Authors.Contains(req.Authors));
        }

        if (!string.IsNullOrWhiteSpace(req.PublishYear))
        {
            predicate = predicate.And(x => x.PublishYear.Contains(req.PublishYear));
        }

        query = query.Where(predicate);

        var pagedBooks = await PagingService.PaginateQueryAsync(query, req, dbContext, ct);

        var bookIds = pagedBooks.Rows.Select(b => b.Id).ToList();

        var lastReadPages = await dbContext.ReadingReports
            .Where(r => r.UserId == userId && bookIds.Contains(r.ReadingResourceId))
            .GroupBy(r => r.ReadingResourceId)
            .Select(g => new
            {
                ReadingResourceId = g.Key,
                LastReadPage = g.OrderByDescending(r => r.ReportDate).First().CurrentPage
            })
            .ToDictionaryAsync(x => x.ReadingResourceId, x => x.LastReadPage, ct);

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
            lastReadPages.GetValueOrDefault(b.Id)
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