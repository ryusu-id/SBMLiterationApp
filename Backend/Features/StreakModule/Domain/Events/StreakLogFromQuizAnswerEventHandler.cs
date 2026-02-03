using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core.Events;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.DailyReadsModule.Domain.Events;

namespace PureTCOWebApp.Features.StreakModule.Domain.Events;

public class StreakLogFromQuizAnswerEventHandler : IDomainEventHandler<QuizAnsweredEvent>
{
    private readonly ApplicationDbContext _dbContext;

    public StreakLogFromQuizAnswerEventHandler(ApplicationDbContext dbContext)
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
        
        var today = DateOnly.FromDateTime(DateTime.UtcNow.ToLocalTime());
        
        var existsToday = await _dbContext.StreakLogs
            .AnyAsync(s => s.UserId == domainEvent.UserId && s.StreakDate == today, cancellationToken);
        
        if (existsToday) return;
        
        var streakLog = StreakLog.Create(domainEvent.UserId, today);
        
        await _dbContext.StreakLogs.AddAsync(streakLog, cancellationToken);
    }
}
