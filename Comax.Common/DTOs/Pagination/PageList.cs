using System.Collections.Generic;

namespace Comax.Common.DTOs.Pagination
{
    public class PagedList<T>
    {
        public int CurrentPage { get; private set; }
        public int TotalPages { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
        public IEnumerable<T> Items { get; private set; } // Danh sách dữ liệu

        public PagedList(IEnumerable<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;
            // Tính tổng số trang (làm tròn lên)
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            Items = items;
        }
    }
}