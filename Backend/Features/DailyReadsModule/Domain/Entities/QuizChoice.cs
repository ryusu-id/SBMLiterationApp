using PureTCOWebApp.Core.Models;

namespace PureTCOWebApp.Features.DailyReadsModule.Domain.Entities;

public class QuizChoice : AuditableEntity
{
    public int Id { get; protected set; }
    public string Choice { get; protected set; }
    public string Answer { get; protected set; }

#pragma warning disable CS8618
    public QuizChoice() { }
#pragma warning restore CS8618

    public static QuizChoice Create(string choice, string answer)
    {
        return new QuizChoice
        {
            Choice = choice,
            Answer = answer
        };
    }
}
