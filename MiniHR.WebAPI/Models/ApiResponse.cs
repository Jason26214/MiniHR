namespace MiniHR.WebAPI.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public ApiError? Error { get; set; }

        public static ApiResponse<T> SuccessResponse(T? data)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Error = null
            };
        }

        public static ApiResponse<T> FailureResponse(string message, int code = 500)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Data = default,
                Error = new ApiError { Code = code, Message = message }
            };
        }
    }

    public class ApiError
    {
        public int Code { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
