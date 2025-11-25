namespace Comax.Common.DTOs.Pagination
{
    public class PaginationParams
    {
        // Giới hạn PageSize tối đa để tránh load quá nhiều data
        private const int MaxPageSize = 50;

        public int PageNumber { get; set; } = 1; // Mặc định là trang 1

        private int _pageSize = 10; // Mặc định là 10 item/trang

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }
    }
}