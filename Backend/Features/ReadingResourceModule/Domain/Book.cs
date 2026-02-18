using PureTCOWebApp.Features.ReadingResourceModule.Domain.Events;

namespace PureTCOWebApp.Features.ReadingResourceModule.Domain;

public class Book : ReadingResourceBase
{
#pragma warning disable CS8618
    public Book() { }
#pragma warning restore CS8618

    public new static Book Create(
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
        var entity = new Book
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

        entity.Raise(new ReadingBookCreatedEvent(entity));

        return entity;
    }

    public new void Update(
        string title,
        string? readingCategory,
        string authors,
        string publishYear,
        int page,
        string cssClass,
        string? coverImageUri = null
    )
    {
        Title = title;
        ReadingCategory = readingCategory;
        Authors = authors;
        PublishYear = publishYear;
        Page = page;
        CssClass = cssClass;
        CoverImageUri = coverImageUri;
    }
}