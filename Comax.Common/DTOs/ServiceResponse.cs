namespace Comax.Common.DTOs
{
    public class ServiceResponse<T>
    {
        public T Data { get; set; }
        public bool Success { get; set; } = true;
        public string Message { get; set; } = null;

        // Helper để trả về lỗi nhanh
        public static ServiceResponse<T> Error(string message)
        {
            return new ServiceResponse<T> { Success = false, Message = message };
        }

        // Helper để trả về thành công nhanh
        public static ServiceResponse<T> Ok(T data, string message = null)
        {
            return new ServiceResponse<T> { Data = data, Success = true, Message = message };
        }
    }
}