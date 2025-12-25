namespace Comax.Common.DTOs
{
    public class AuthResultDTO
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
        public string? Username { get; set; }
        public int UserId { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public DateTime? CreatedAt { get; set; }
        public Guid RowVersion { get; set; }
    }
}
