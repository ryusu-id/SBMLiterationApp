using PureTCOWebApp.Core.Models;

namespace PureTCOWebApp.Features.ReadingRecommendationModule.Domain;

public class ReadingRecommendation : AuditableEntity
{
    public int Id { get; protected set; }
    public string Title { get; protected set; }
    public string ISBN { get; protected set; }
    public string ReadingCategory { get; protected set; }
    public string Authors { get; protected set; }
    public string PublishYear { get; protected set; }
    public int Page { get; protected set; }
    public string? ResourceLink { get; protected set; }
    public string? CoverImageUri { get; protected set; }

#pragma warning disable CS8618
    public ReadingRecommendation() { }
#pragma warning restore CS8618

    public static ReadingRecommendation Create(
        string title,
        string isbn,
        string readingCategory,
        string authors,
        string publishYear,
        int page,
        string? resourceLink = null,
        string? coverImageUri = null
    )
    {
        return new ReadingRecommendation
        {
            Title = title,
            ISBN = isbn,
            ReadingCategory = readingCategory,
            Authors = authors,
            PublishYear = publishYear,
            Page = page,
            ResourceLink = resourceLink,
            CoverImageUri = coverImageUri
        };
    }

    public void Update(
        string title,
        string isbn,
        string readingCategory,
        string authors,
        string publishYear,
        int page,
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
        ResourceLink = resourceLink;
        CoverImageUri = coverImageUri;
    }
}
