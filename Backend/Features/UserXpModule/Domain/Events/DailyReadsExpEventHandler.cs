using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core.Events;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.DailyReadsModule.Domain.Events;

namespace PureTCOWebApp.Features.UserXpModule.Domain.Events;

public class DailyReadsExpEventHandler : IDomainEventHandler<QuizAnsweredEvent>
{
    private readonly ApplicationDbContext _dbContext;

    public DailyReadsExpEventHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(QuizAnsweredEvent domainEvent, CancellationToken cancellationToken)
    {
        var latestAnswers = await _dbContext.QuizAnswers
            .Where(a => a.UserId == domainEvent.UserId && a.DailyReadId == domainEvent.DailyRead.Id)
            .GroupBy(a => a.QuestionSeq)
            .Select(g => g.OrderByDescending(a => a.RetrySeq).First())
            .ToListAsync(cancellationToken);
        
        var questions = await _dbContext.QuizQuestions
            .Where(q => q.DailyReadId == domainEvent.DailyRead.Id)
            .ToListAsync(cancellationToken);
        
        var dailyRead = await _dbContext.DailyReads
            .FirstOrDefaultAsync(dr => dr.Id == domainEvent.DailyRead.Id, cancellationToken);
        
        if (dailyRead == null) return;
        
        var correctCount = latestAnswers.Count(a =>
            questions.Any(q => q.QuestionSeq == a.QuestionSeq &&
                               q.CorrectAnswer.Equals(a.Answer, StringComparison.OrdinalIgnoreCase)));
        
        if (correctCount < dailyRead.MinimalCorrectAnswer) return;
        
        var alreadyGivenXp = await _dbContext.UserExpEvents
            .AnyAsync(e => e.UserId == domainEvent.UserId &&
                          e.EventName == nameof(UserExpEvent.ExpEventType.DailyReadsExp) &&
                          e.RefId == domainEvent.DailyRead.Id, cancellationToken);
        
        if (alreadyGivenXp) return;

        var exp = await _dbContext
            .DailyReads
            .Where(e => e.Id == domainEvent.DailyRead.Id)
            .Select(e => e.Exp)
            .FirstOrDefaultAsync(cancellationToken);

        if (exp == 0) return;

        var userExp = UserExpEvent.Create(
            domainEvent.UserId,
            exp,
            nameof(UserExpEvent.ExpEventType.DailyReadsExp),
            domainEvent.DailyRead.Id
        );
        
        await _dbContext.UserExpEvents.AddAsync(userExp, cancellationToken);
    }
}
