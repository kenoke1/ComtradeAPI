namespace ComtradeAPI.ModelDTO
{
    public class ServiceResult<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string> Errors { get; set; } = new();

        public static ServiceResult<T> Success(T data) => new()
        {
            IsSuccess = true,
            Data = data
        };

        public static ServiceResult<T> Failure(string error) => new()
        {
            IsSuccess = false,
            ErrorMessage = error,
            Errors = new List<string> { error }
        };
    }
}
