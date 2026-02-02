using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Core.Paging;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.DailyReadsModule.Endpoints.DailyReadEndpoints;

public record QuizResultInfo(
    int TotalQuestions,
    int CorrectAnswers,
    bool HasAttempted,
    bool HasPassed
);

public record DailyReadWithQuizResult(
    int Id,
    string Title,
    string? CoverImg,
    string Content,
    DateOnly Date,
    string? Category,
    decimal Exp,
    int MinimalCorrectAnswer,
    QuizResultInfo? QuizResult
);

public record GetDailyReadsForParticipantRequest(
    string? Title = null,
    string? Category = null,
    DateOnly? DateTo = null
) : PagingQuery;

public class GetDailyReadsForParticipantEndpoint(ApplicationDbContext dbContext)
    : Endpoint<GetDailyReadsForParticipantRequest, PagingResult<DailyReadWithQuizResult>>
{
    public override void Configure()
    {
        Get("participant");
        Group<DailyReadsEndpointGroup>();
    }

    public override async Task HandleAsync(GetDailyReadsForParticipantRequest req, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);

        var query = dbContext.DailyReads.AsQueryable();

        var predicate = PredicateBuilder.True<Domain.DailyRead>();

        if (!string.IsNullOrWhiteSpace(req.Title))
        {
            predicate = predicate.And(x => x.Title.Contains(req.Title));
        }

        if (!string.IsNullOrWhiteSpace(req.Category))
        {
            predicate = predicate.And(x => x.Category != null && x.Category.Contains(req.Category));
        }

        var localTime = DateTime.UtcNow.ToLocalTime();
        var today = DateOnly.FromDateTime(DateTime.UtcNow.ToLocalTime());
        if (req.DateTo.HasValue)
        {
            predicate = predicate.And(x => x.Date <= req.DateTo.Value);
        }
        else
        {
            predicate = predicate.And(x => x.Date == today);
        }

        predicate = predicate.And(x => x.Date <= today);

        query = query.Where(predicate).OrderByDescending(x => x.Date);

        var pagedDailyReads = await PagingService.PaginateQueryAsync(query, req, dbContext, ct);

        var dailyReadIds = pagedDailyReads.Rows.Select(dr => dr.Id).ToList();

        var userAnswers = await dbContext.QuizAnswers
            .Where(a => a.UserId == userId && dailyReadIds.Contains(a.DailyReadId))
            .GroupBy(a => new { a.DailyReadId, a.QuestionSeq })
            .Select(g => new
            {
                g.Key.DailyReadId,
                g.Key.QuestionSeq,
                LatestAnswer = g.OrderByDescending(a => a.RetrySeq).First()
            })
            .ToListAsync(ct);

        var quizQuestions = await dbContext.QuizQuestions
            .Where(q => dailyReadIds.Contains(q.DailyReadId))
            .ToListAsync(ct);

        var itemsWithQuizResult = pagedDailyReads.Rows.Select(dr =>
        {
            var questions = quizQuestions.Where(q => q.DailyReadId == dr.Id).ToList();
            var answers = userAnswers.Where(a => a.DailyReadId == dr.Id).ToList();

            QuizResultInfo? quizResult = null;

            if (questions.Any())
            {
                var correctCount = answers.Count(a =>
                    questions.Any(q => q.QuestionSeq == a.QuestionSeq &&
                                       q.CorrectAnswer.Equals(a.LatestAnswer.Answer, StringComparison.OrdinalIgnoreCase)));

                quizResult = new QuizResultInfo(
                    questions.Count,
                    correctCount,
                    answers.Any(),
                    correctCount >= dr.MinimalCorrectAnswer
                );
            }

            return new DailyReadWithQuizResult(
                dr.Id,
                dr.Title,
                dr.CoverImg,
                dr.Content,
                dr.Date,
                dr.Category,
                dr.Exp,
                dr.MinimalCorrectAnswer,
                quizResult
            );
        }).ToList();

        var result = new PagingResult<DailyReadWithQuizResult>(
            itemsWithQuizResult,
            pagedDailyReads.Page,
            pagedDailyReads.RowsPerPage,
            pagedDailyReads.TotalRows,
            pagedDailyReads.TotalPages,
            pagedDailyReads.SearchText,
            pagedDailyReads.SortBy,
            pagedDailyReads.SortDirection
        );

        await Send.OkAsync(result, ct);
    }
}
