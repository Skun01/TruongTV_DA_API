namespace Application.Helper;

public static class PagingHelper
{
    public static (int Page, int PageSize) Normalize(int page, int pageSize, int defaultPageSize = 20, int maxPageSize = 100)
    {
        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = pageSize <= 0 ? defaultPageSize : Math.Min(pageSize, maxPageSize);
        return (normalizedPage, normalizedPageSize);
    }
}
