using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Features.ReadingResourceModule.Domain.Events;
using System.Text.Json.Serialization;

namespace PureTCOWebApp.Features.ReadingResourceModule.Domain.Entities;

public class ReadingReport : AuditableEntity
{
    public int Id { get; protected set; }
    public int UserId { get; protected set; }
    public int ReadingResourceId { get; protected set; }
    public DateTime ReportDate { get; protected set; }
    public int CurrentPage { get; protected set; }
    public string Insight { get; protected set; }
    public int TimeSpent { get; protected set; }

    [JsonIgnore]
    public ReadingResourceBase ReadingResource { get; protected set; }

#pragma warning disable CS8618
    public ReadingReport() { }
#pragma warning restore CS8618

    public static ReadingReport Create(
        int userId,
        int readingResourceId,
        int currentPage,
        string insight,
        int timeSpent,
        int lastPage = 0
    )
    {
        var entity = new ReadingReport
        {
            UserId = userId,
            ReadingResourceId = readingResourceId,
            CurrentPage = currentPage,
            Insight = insight,
            TimeSpent = timeSpent,
            ReportDate = DateTime.UtcNow
        };

        entity.Raise(new ReadingReportCreatedEvent(entity, lastPage));

        return entity;
    }

    public void Update(string insight, int? timeSpent = null)
    {
        Insight = insight;
        if (timeSpent.HasValue)
            TimeSpent = timeSpent.Value;
    }
}