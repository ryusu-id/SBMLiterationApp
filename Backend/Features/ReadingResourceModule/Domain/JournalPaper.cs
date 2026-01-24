namespace PureTCOWebApp.Features.ReadingResourceModule.Domain;

public class JournalPaper : ReadingResourceBase
{
#pragma warning disable CS8618
    public JournalPaper() { }
#pragma warning restore CS8618

    public new static JournalPaper Create(
        int userId,
        string title,
        string isbn,
        string readingCategory,
        string authors,
        string publishYear,
        int page,
        string cssClass,
        string? resourceLink = null,
        string? coverImageUri = null
    )
    {
        return new JournalPaper
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

    public new void Update(
        string title,
        string isbn,
        string readingCategory,
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