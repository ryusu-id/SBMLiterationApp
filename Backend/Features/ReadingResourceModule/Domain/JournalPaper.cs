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
        string bookCategory,
        string authors,
        string publishYear,
        int page,
        string? resourceLink = null,
        string? coverImageUri = null)
    {
        return new JournalPaper
        {
            UserId = userId,
            Title = title,
            ISBN = isbn,
            BookCategory = bookCategory,
            Authors = authors,
            PublishYear = publishYear,
            Page = page,
            ResourceLink = resourceLink,
            CoverImageUri = coverImageUri
        };
    }

    public new void Update(
        string title,
        string isbn,
        string bookCategory,
        string authors,
        string publishYear,
        int page,
        string? resourceLink = null,
        string? coverImageUri = null)
    {
        Title = title;
        ISBN = isbn;
        BookCategory = bookCategory;
        Authors = authors;
        PublishYear = publishYear;
        Page = page;
        ResourceLink = resourceLink;
        CoverImageUri = coverImageUri;
    }
}