namespace PureTCOWebApp.Core.Paging;

public class RangeQuery<T>
{
    public T? From { get; set; } = default(T);
    public T? To { get; set; } = default(T);
}
