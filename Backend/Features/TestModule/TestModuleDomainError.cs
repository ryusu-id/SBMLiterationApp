using PureTCOWebApp.Core.Models;

namespace PureTCOWebApp.Features.TestModule;

public static class TestModuleDomainError
{
    public static readonly Error TestItemNotFound = new("TestModule.TestItemNotFound", "Test item not found");
    public static readonly Error TestItemNameRequired = new("TestModule.TestItemNameRequired", "Test item name is required");
    public static readonly Error TestItemNameTooLong = new("TestModule.TestItemNameTooLong", "Test item name cannot exceed 100 characters");
    public static readonly Error TestItemPriceInvalid = new("TestModule.TestItemPriceInvalid", "Test item price must be greater than 0");
}