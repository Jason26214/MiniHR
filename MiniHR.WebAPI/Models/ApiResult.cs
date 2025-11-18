namespace MiniHR.WebAPI.Models
{
    public class ApiResult<T>
    {
        public bool Success { get; set; }

        public int Code { get; set; }

        public T? Data { get; set; }

        public object? Error { get; set; }

        public string? Message { get; set; }
    }
}