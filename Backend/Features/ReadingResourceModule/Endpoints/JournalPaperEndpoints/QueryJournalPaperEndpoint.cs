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
    string ReadingCategory,
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

        var queryWithLastRead = from journal in query
                                select new
                                {
                                    Journal = journal,
                                    LastReadDate = dbContext.ReadingReports
                                        .Where(r => r.ReadingResourceId == journal.Id)
                                        .OrderByDescending(r => r.ReportDate)
                                        .Select(r => (DateTime?)r.ReportDate)
                                        .FirstOrDefault()
                                };

        queryWithLastRead = queryWithLastRead.OrderByDescending(x => x.LastReadDate);

        var pagedJournals = await PagingService.PaginateQueryAsync(queryWithLastRead.Select(x => x.Journal), req, dbContext, ct);

        var journalIds = pagedJournals.Rows.Select(j => j.Id).ToList();

        var lastReadPages = await dbContext.ReadingReports
            .Where(r => r.UserId == userId && journalIds.Contains(r.ReadingResourceId))
            .GroupBy(r => r.ReadingResourceId)
            .Select(g => new
            {
                ReadingResourceId = g.Key,
                LastReadPage = g.OrderByDescending(r => r.ReportDate).First().CurrentPage
            })
            .ToDictionaryAsync(x => x.ReadingResourceId, x => x.LastReadPage, ct);

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
            lastReadPages.GetValueOrDefault(j.Id)
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