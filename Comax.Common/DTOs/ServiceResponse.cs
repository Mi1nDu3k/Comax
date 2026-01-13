namespace Comax.Common.DTOs
{
    public class ServiceResponse<T>
    {
        public T Data { get; set; }
        public bool Success { get; set; } = true;
        public string Message { get; set; } = null;

        public static ServiceResponse<T> Error(string message)
        {
            return new ServiceResponse<T> { Success = false, Message = message };
        }

        public static ServiceResponse<T> Ok(T data, string message = null)
        {
            return new ServiceResponse<T> { Data = data, Success = true, Message = message };
        }
    }
}