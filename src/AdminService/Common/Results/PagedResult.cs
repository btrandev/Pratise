namespace AdminService.Common.Results;

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; }
    public int TotalCount { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalPages { get; }
    
    public PagedResult(IEnumerable<T> items, int totalCount, int page, int pageSize, int totalPages)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
        TotalPages = totalPages;
    }
}
