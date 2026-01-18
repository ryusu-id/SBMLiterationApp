using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Core.Paging;

public static class PagingService
{
    public static async Task<PagingResult<M>> PaginateQueryAsync<T, M>(
        IQueryable<T> sourceQry,
        PagingQuery filter,
        Func<T, M> mappingFunction,
        CancellationToken ct)
        where T : class
        where M : class
    {
        var sortBy = ConvertSortByParams(filter.SortBy ?? "-CreateTime");

        var qry = OrderByStringValues(sourceQry, sortBy).AsNoTracking();

        int itemCount = await sourceQry.CountAsync(ct);
        int pageCount = (int)Math.Ceiling(itemCount / (double)(filter.RowsPerPage));
        int pageIndex = (filter.Page) > pageCount ? pageCount : (filter.Page);

        if (pageIndex > 0)
        {
            qry = qry.Skip((pageIndex - 1) * (filter.RowsPerPage))
                .Take((filter.RowsPerPage));
        }
        else
        {
            qry = qry.Take((filter.RowsPerPage));
        }

        return new PagingResult<M>(
            (await qry
                .ToListAsync())
                .Select(mappingFunction)
                .ToList(),
            filter.Page,
            filter.RowsPerPage,
            itemCount,
            pageCount,
            filter.Search ?? "",
            filter.SortBy ?? "",
            filter.SortDescending ?? ""
        );
    }
    public static async Task<PagingResult<T>> PaginateQueryAsync<T>(
        IQueryable<T> sourceQry,
        PagingQuery filter,
        CancellationToken ct)
        where T : class
    {
        var sortBy = ConvertSortByParams(!string.IsNullOrEmpty(filter.SortBy) ? filter.SortBy : "-CreateTime");

        var qry = OrderByStringValues(sourceQry, sortBy).AsNoTracking();

        int itemCount = await sourceQry.CountAsync(ct);
        int pageCount = (int)Math.Ceiling(itemCount / (double)(filter.RowsPerPage));
        int pageIndex = (filter.Page) > pageCount ? pageCount : (filter.Page);

        if (pageIndex > 0)
        {
            qry = qry.Skip((pageIndex - 1) * (filter.RowsPerPage))
                .Take((filter.RowsPerPage));
        }
        else
        {
            qry = qry.Take((filter.RowsPerPage));
        }

        return new PagingResult<T>(
            await qry.ToListAsync(),
            filter.Page,
            filter.RowsPerPage,
            itemCount,
            pageCount,
            filter.Search ?? "",
            filter.SortBy ?? "",
            filter.SortDescending ?? ""
        );
    }

    public static async Task<PagingResult<T>> PaginateQueryAsync<T>(
        IQueryable<T> sourceQry,
        PagingQuery filter,
        ApplicationDbContext dbContext,
        CancellationToken ct)
        where T : AuditableEntity, new()
    {
        var sortBy = ConvertSortByParams(filter.SortBy ?? "-CreateTime");

        var qry = OrderByStringValues(sourceQry, sortBy).AsNoTracking();

        int itemCount = await sourceQry.CountAsync(ct);
        int pageCount = (int)Math.Ceiling(itemCount / (double)(filter.RowsPerPage));
        int pageIndex = (filter.Page) > pageCount ? pageCount : (filter.Page);

        if (pageIndex > 0)
        {
            qry = qry.Skip((pageIndex - 1) * (filter.RowsPerPage))
                .Take((filter.RowsPerPage));
        }
        else
        {
            qry = qry.Take((filter.RowsPerPage));
        }

        qry = qry.IncludeAuditStrings(dbContext);

        return new PagingResult<T>(
            await qry.ToListAsync(),
            filter.Page,
            filter.RowsPerPage,
            itemCount,
            pageCount,
            filter.Search ?? "",
            filter.SortBy ?? "",
            filter.SortDescending ?? ""
        );
    }

    public static async Task<PagingResult<T>> LoadQueryAsync<T>(
        IQueryable<T> sourceQry,
        PagingQuery filter,
        ApplicationDbContext dbContext,
        CancellationToken ct)
        where T : class
    {
        var sortBy = ConvertSortByParams(filter.SortBy ?? "-CreateTime");

        var qry = OrderByStringValues(sourceQry, sortBy).AsNoTracking();

        int itemCount = await sourceQry.CountAsync(ct);
        int pageCount = (int)Math.Ceiling(itemCount / (double)(filter.RowsPerPage));
        int pageIndex = (filter.Page) > pageCount ? pageCount : (filter.Page);

        if (pageIndex > 0)
        {
            qry = qry.Skip((pageIndex - 1) * (filter.RowsPerPage))
                .Take((filter.RowsPerPage));
        }
        else
        {
            qry = qry.Take((filter.RowsPerPage));
        }

        return new PagingResult<T>(
            await qry.ToListAsync(),
            filter.Page,
            filter.RowsPerPage,
            itemCount,
            pageCount,
            filter.Search ?? "",
            filter.SortBy ?? "",
            filter.SortDescending ?? ""
        );
    }

    private static string ConvertSortByParams(string sortByFields)
    {
        string orderByStr = "";
        if (!string.IsNullOrEmpty(sortByFields))
        {
            List<string> sortFields = sortByFields.Trim().Split(',').Select(x => x.Trim()).ToList();
            for (var i = 0; i < sortFields.Count; i++)
            {
                bool isDescending = sortFields[i].First() == '-';

                if (orderByStr != "")
                    orderByStr += ", ";

                if (isDescending)
                {
                    sortFields[i] = sortFields[i][1..];
                }

                orderByStr = orderByStr + sortFields[i].First().ToString().ToUpper() + sortFields[i][1..];

                if (isDescending)
                    orderByStr += " DESC";
                else
                    orderByStr += " ASC";
            }
        }
        return orderByStr;
    }

    private static IQueryable<T> OrderByStringValues<T>(IQueryable<T> source, string orderByStrValues)
    {
        var queryExpr = source.Expression;
        var methodAsc = "OrderBy";
        var methodDesc = "OrderByDescending";

        var orderByValues = orderByStrValues.Trim().Split(',').Select(x => x.Trim()).ToList();

        foreach (var orderPairCommand in orderByValues)
        {
            var command = orderPairCommand.ToUpper().EndsWith(" DESC") ? methodDesc : methodAsc;

            //Get propertyname and remove optional ASC or DESC
            var propertyName = orderPairCommand.Split(' ')[0].Trim();

            var type = typeof(T);
            var parameter = Expression.Parameter(type, "p");

            PropertyInfo? property;
            MemberExpression propertyAccess;

            if (propertyName.Contains('.'))
            {
                // support to be sorted on child fields. 
                var childProperties = propertyName.Split('.');

                property = SearchProperty(typeof(T), childProperties[0]);

                if (property is null)
                    continue;

                propertyAccess = Expression.MakeMemberAccess(parameter, property);

                for (int i = 1; i < childProperties.Length; i++)
                {
                    var t = property.PropertyType;

                    property = SearchProperty(t, childProperties[i]);

                    if (property == null)
                        break;

                    propertyAccess = Expression.MakeMemberAccess(propertyAccess, property);
                }
            }
            else
            {
                property = null;
                property = SearchProperty(type, propertyName);

                if (property == null)
                    continue;

                propertyAccess = Expression.MakeMemberAccess(parameter, property);
            }

            var orderByExpression = Expression.Lambda(propertyAccess, parameter);

            if (property is null)
                continue;
            queryExpr = Expression.Call(typeof(Queryable), command, new Type[] { type, property.PropertyType }, queryExpr, Expression.Quote(orderByExpression));
            methodAsc = "ThenBy";
            methodDesc = "ThenByDescending";
        }
        return source.Provider.CreateQuery<T>(queryExpr);
    }

    private static PropertyInfo? SearchProperty(Type type, string propertyName)
    {
        foreach (var item in type.GetProperties())
            if (item.Name.ToLower() == propertyName.ToLower())
                return item;
        return null;
    }
}
