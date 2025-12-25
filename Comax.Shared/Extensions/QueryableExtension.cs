namespace Comax.Shared.Extensions
{
    public static class QueryableExtension
    {
        public static IQueryable<T> ToPaginationAsync<T>(this IQueryable<T> query, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            int skip = (pageNumber - 1) * pageSize;
            return query.Skip(skip).Take(pageSize);
        }
    }
}
    