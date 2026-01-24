using PureTCOWebApp.Core.Models;

namespace PureTCOWebApp.Features.ReadingCategoryModule.Domain;

public class ReadingCategory : AuditableEntity
{
    public int Id { get; protected set; }
    public string CategoryName { get; protected set; }

#pragma warning disable CS8618
    public ReadingCategory() { }
#pragma warning restore CS8618

    public static ReadingCategory Create(string categoryName)
    {
        return new ReadingCategory
        {
            CategoryName = categoryName
        };
    }

    public void Update(string categoryName)
    {
        CategoryName = categoryName;
    }
}
