using Microsoft.AspNetCore.Http; // Cần reference gói Microsoft.AspNetCore.Http.Features

namespace Comax.Common.DTOs.Comic
{
    public class ComicCreateDTO : BaseDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int AuthorId { get; set; }
        public string CategoryID { get; set; } // Chuỗi JSON hoặc list ID

        // --- THÊM ---
        public IFormFile? CoverImageFile { get; set; } // File ảnh gửi lên từ form
    }
}