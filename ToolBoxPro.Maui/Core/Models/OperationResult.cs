namespace ToolBoxPro.Core.Models
{
    public class OperationResult<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public Exception? Exception { get; set; }

        public static OperationResult<T> Ok(T data, string? message = null)
        {
            return new OperationResult<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        public static OperationResult<T> Fail(string message, Exception? exception = null)
        {
            return new OperationResult<T>
            {
                Success = false,
                Message = message,
                Exception = exception
            };
        }
    }

    public class OperationResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public Exception? Exception { get; set; }

        public static OperationResult Ok(string? message = null)
        {
            return new OperationResult
            {
                Success = true,
                Message = message
            };
        }

        public static OperationResult Fail(string message, Exception? exception = null)
        {
            return new OperationResult
            {
                Success = false,
                Message = message,
                Exception = exception
            };
        }
    }
}
