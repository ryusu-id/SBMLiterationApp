using PureTCOWebApp.Core.Models;

namespace PureTCOWebApp.Data;

public static class AuditableEntityExtensions
{
    public static IQueryable<T> IncludeAuditStrings<T>(
        this IQueryable<T> query, ApplicationDbContext dbContext)
    where T : IAuditableEntity, new()
    {
        // Perform a join with the User entity to fetch the names
        return from entity in query
               join createdByUser in dbContext.Users on entity.CreateBy equals createdByUser.Id into createdByGroup
               from createdBy in createdByGroup.DefaultIfEmpty()
               join updatedByUser in dbContext.Users on entity.UpdateBy equals updatedByUser.Id into updatedByGroup
               from updatedBy in updatedByGroup.DefaultIfEmpty()
               select CreateEntityWithAuditStrings(entity, createdBy.UserName, updatedBy.UserName);
    }
    private static T CreateEntityWithAuditStrings<T>(T entity, string? createByStr, string? updateByStr) where T : IAuditableEntity, new()
    {
        // Copy all properties from the original entity to the new instance
        entity.Status = entity.Status;
        entity.CreateTime = entity.CreateTime;
        entity.UpdateTime = entity.UpdateTime;
        entity.CreateBy = entity.CreateBy;
        entity.UpdateBy = entity.UpdateBy;

        // Set the additional fields
        entity.CreateByStr = createByStr;
        entity.UpdateByStr = updateByStr;

        return entity;
    }
}
