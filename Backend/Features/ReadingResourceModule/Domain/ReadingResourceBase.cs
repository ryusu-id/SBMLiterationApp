using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Features.ReadingResourceModule.Domain.Entities;
using System.Text.Json.Serialization;

namespace PureTCOWebApp.Features.ReadingResourceModule.Domain;

public class ReadingResourceBase : AuditableEntity
{
    public int Id { get; protected set; }
    public int UserId { get; protected set; }
    public string Title { get; protected set; }
    public string ISBN { get; protected set; }
    public string? ReadingCategory { get; protected set; }
    public string Authors { get; protected set; }
    public string PublishYear { get; protected set; }
    public int Page { get; protected set; }
    public string? ResourceLink { get; protected set; }
    public string? CoverImageUri { get; protected set; }
    public string CssClass { get; protected set; }

    [JsonIgnore]
    public virtual ICollection<ReadingReport> ReadingReports { get; set; } = [];

    public bool CanBeDeleted() => !ReadingReports.Any();

#pragma warning disable CS8618
    public ReadingResourceBase() { }
#pragma warning restore CS8618

    public static ReadingResourceBase Create(
        int userId,
        string title,
        string isbn,
        string? readingCategory,
        string authors,
        string publishYear,
        int page,
        string cssClass,
        string? resourceLink = null,
        string? coverImageUri = null
    )
    {
        return new ReadingResourceBase
        {
            UserId = userId,
            Title = title,
            ISBN = isbn,
            ReadingCategory = readingCategory,
            Authors = authors,
            PublishYear = publishYear,
            Page = page,
            CssClass = cssClass,
            ResourceLink = resourceLink,
            CoverImageUri = coverImageUri
        };
    }

    public void Update(
        string title,
        string isbn,
        string? readingCategory,
        string authors,
        string publishYear,
        int page,
        string cssClass,
        string? resourceLink = null,
        string? coverImageUri = null
    )
    {
        Title = title;
        ISBN = isbn;
        ReadingCategory = readingCategory;
        Authors = authors;
        PublishYear = publishYear;
        Page = page;
        CssClass = cssClass;
        ResourceLink = resourceLink;
        CoverImageUri = coverImageUri;
    }
}