using PureTCOWebApp.Core.Models;

namespace PureTCOWebApp.Features.DailyReadsModule.Domain;

public class DailyRead : AuditableEntity
{
    public int Id { get; protected set; }
    public string Title { get; protected set; }
    public string? CoverImg { get; protected set; }
    public string Content { get; protected set; }
    public DateOnly Date { get; protected set; }
    public string? Category { get; protected set; }
    public decimal Exp { get; protected set; }
    public int MinimalCorrectAnswer { get; protected set; }

#pragma warning disable CS8618
    public DailyRead() { }
#pragma warning restore CS8618

    public static DailyRead Create(string title, string content, DateOnly date, string? coverImg = null, string? category = null, decimal exp = 0, int minimalCorrectAnswer = 0)
    {
        return new DailyRead
        {
            Title = title,
            Content = content,
            Date = date,
            CoverImg = coverImg,
            Category = category,
            Exp = exp,
            MinimalCorrectAnswer = minimalCorrectAnswer
        };
    }

    public void Update(string title, string content, DateOnly date, string? coverImg = null, string? category = null, decimal exp = 0, int minimalCorrectAnswer = 0)
    {
        Title = title;
        Content = content;
        Date = date;
        CoverImg = coverImg;
        Category = category;
        Exp = exp;
        MinimalCorrectAnswer = minimalCorrectAnswer;
    }
}
