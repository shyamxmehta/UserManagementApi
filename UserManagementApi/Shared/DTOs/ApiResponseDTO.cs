namespace UserManagementApi.Shared.DTOs
{
    public class ApiResponseDTO<T>
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; }
        public T? Data { get; set; }
        public object? Errors { get; set; }

        //add metadata
    }
}
