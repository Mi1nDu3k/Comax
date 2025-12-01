namespace Comax.Common.DTOs.Pagination
{
    public class PaginationParams
    {
        // Giới hạn PageSize tối đa để tránh load quá nhiều data
        private const int MaxPageSize = 50;

        /// <summary>
        /// Mặc định là trang 1
        /// </summary>
        public int PageNumber { get; set; } = 1; 

        /// <summary>
        /// Mặc định là 10 item trên mỗi trang
        /// </summary>
        public int PageSize { get; set; } = 10; 
    }
}