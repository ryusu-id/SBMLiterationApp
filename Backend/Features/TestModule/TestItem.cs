using PureTCOWebApp.Core.Models;

namespace PureTCOWebApp.Features.TestModule;

public class TestItem : AuditableEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public TestItem()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
    }

    private TestItem(
        string name,
        string? description,
        decimal price
    )
    {
        Name = name;
        Description = description;
        Price = price;
        IsActive = true;
    }

    public void Update(string name, string? description, decimal price)
    {
        Name = name;
        Description = description;
        Price = price;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public static TestItem Create(string name, string? description, decimal price)
    {
        return new TestItem(name, description, price);
    }
}