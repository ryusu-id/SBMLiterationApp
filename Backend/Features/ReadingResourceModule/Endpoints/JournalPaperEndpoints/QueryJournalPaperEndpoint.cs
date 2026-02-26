using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core.Paging;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.ReadingResourceModule.Domain;

namespace PureTCOWebApp.Features.ReadingResourceModule.Endpoints.JournalPaperEndpoints;

public record JournalPaperWithProgress(
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

public record QueryJournalPaperRequest(
    string? Title = null,
    string? ISBN = null,
    string? ReadingCategory = null,
    string? Authors = null,
    string? PublishYear = null
) : PagingQuery;

public class QueryJournalPaperEndpoint(ApplicationDbContext dbContext)
    : Endpoint<QueryJournalPaperRequest, PagingResult<JournalPaperWithProgress>>
{
    public override void Configure()
    {
        Get("journals");
        Group<ReadingResourceEndpointGroup>();
    }

    public override async Task HandleAsync(QueryJournalPaperRequest req, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        
        var query = dbContext.JournalPapers.Where(x => x.UserId == userId);

        var predicate = PredicateBuilder.True<JournalPaper>();

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

        var queryWithLastRead = from journal in query
                                select new
                                {
                                    Journal = journal,
                                    LastReadDate = dbContext.ReadingReports
                                        .Where(r => r.ReadingResourceId == journal.Id)
                                        .OrderByDescending(r => r.ReportDate)
                                        .Select(r => (DateTime?)r.ReportDate)
                                        .FirstOrDefault(),
                                    LastReadPage = dbContext.ReadingReports
                                        .Where(r => r.ReadingResourceId == journal.Id)
                                        .OrderByDescending(r => r.ReportDate)
                                        .Select(r => (int?)r.CurrentPage)
                                        .FirstOrDefault()
                                };

        queryWithLastRead = queryWithLastRead.OrderByDescending(x => x.LastReadDate);

        // Set SortBy to empty to preserve our custom ordering
        req = req with { SortBy = "" };

        var pagedJournals = await PagingService.PaginateQueryAsync(queryWithLastRead.Select(x => x.Journal), req, dbContext, ct);

        var journalIds = pagedJournals.Rows.Select(j => j.Id).ToList();
        var lastReadData = await queryWithLastRead
            .Where(x => journalIds.Contains(x.Journal.Id))
            .ToDictionaryAsync(x => x.Journal.Id, x => x.LastReadPage, ct);

        var journalsWithProgress = pagedJournals.Rows.Select(j => new JournalPaperWithProgress(
            j.Id,
            j.UserId,
            j.Title,
            j.ISBN,
            j.ReadingCategory,
            j.Authors,
            j.PublishYear,
            j.Page,
            j.ResourceLink,
            j.CoverImageUri,
            j.CssClass,
            lastReadData.GetValueOrDefault(j.Id)
        )).ToList();

        var result = new PagingResult<JournalPaperWithProgress>(
            journalsWithProgress,
            pagedJournals.Page,
            pagedJournals.RowsPerPage,
            pagedJournals.TotalRows,
            pagedJournals.TotalPages,
            pagedJournals.SearchText,
            pagedJournals.SortBy,
            pagedJournals.SortDirection
        );

        await Send.OkAsync(result, cancellation: ct);
    }
}
