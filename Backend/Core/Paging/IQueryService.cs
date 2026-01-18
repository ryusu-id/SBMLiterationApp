namespace PureTCOWebApp.Core.Paging;
public interface IQueryService
{
    public Task<ICollection<T>> LoadQueryAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default) where T : class;
}
