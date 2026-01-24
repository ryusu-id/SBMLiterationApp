using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using PureTCOWebApp.Core.Events;
using PureTCOWebApp.Core.JsonConverter;

namespace PureTCOWebApp.Core.Models;

public interface IAuditableEntity
{
    int Status { get; set; }
    DateTime CreateTime { get; set; }
    DateTime? UpdateTime { get; set; }
    long CreateBy { get; set; }
    long? UpdateBy { get; set; }
    string? CreateByStr { get; set; }
    string? UpdateByStr { get; set; }
}

public abstract class AuditableEntity : IAuditableEntity
{
    public int Status { get; set; } = 0;
    [JsonConverter(typeof(DateTimeJsonConverter))]
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    [JsonConverter(typeof(DateTimeJsonConverter))]
    public DateTime? UpdateTime { get; set; }
    [JsonIgnore]
    public long CreateBy { get; set; }
    [JsonIgnore]
    public long? UpdateBy { get; set; }

    [JsonPropertyName("createBy")]
    public string? CreateByStr { get; set; }
    [JsonPropertyName("updateBy")]
    public string? UpdateByStr { get; set; }


    [NotMapped]
    private List<IDomainEvent> _domainEvents = new();
    [JsonIgnore]
    [NotMapped]
    public List<IDomainEvent> DomainEvents => [.._domainEvents];
    public void Raise(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
