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
        string bookCategory,
        string authors,
        string publishYear,
        int page,
        string? resourceLink = null)
    {
        return new Book
        {
            UserId = userId,
            Title = title,
            ISBN = isbn,
            BookCategory = bookCategory,
            Authors = authors,
            PublishYear = publishYear,
            Page = page,
            ResourceLink = resourceLink
        };
    }

    public void Update(
        int userId,
        string title,
        string isbn,
        string bookCategory,
        string authors,
        string publishYear,
        int page,
        string? resourceLink = null)
    {
        UserId = userId;
        Title = title;
        ISBN = isbn;
        BookCategory = bookCategory;
        Authors = authors;
        PublishYear = publishYear;
        Page = page;
        ResourceLink = resourceLink;
    }
}