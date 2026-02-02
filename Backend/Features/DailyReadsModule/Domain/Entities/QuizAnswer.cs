using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Features.DailyReadsModule.Domain.Events;

namespace PureTCOWebApp.Features.DailyReadsModule.Domain.Entities;

public class QuizAnswer : AuditableEntity
{
    public int Id { get; protected set; }
    public int UserId { get; protected set; }
    public int DailyReadId { get; protected set; }
    public int QuestionSeq { get; protected set; }
    public string Answer { get; protected set; }
    public int RetrySeq { get; protected set; }

#pragma warning disable CS8618
    public QuizAnswer() { }
#pragma warning restore CS8618

    public static QuizAnswer Create(int userId, int dailyReadId, int questionSeq, string answer, int retrySeq = 0)
    {
        var entity = new QuizAnswer
        {
            UserId = userId,
            DailyReadId = dailyReadId,
            QuestionSeq = questionSeq,
            Answer = answer,
            RetrySeq = retrySeq
        };
        
        return entity;
    }
}
