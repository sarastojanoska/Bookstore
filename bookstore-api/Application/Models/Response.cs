using System.Net;

namespace Application.Models
{
    public class Response<T>
    {
        public bool Success { get; set; }
        public string? Error { get; set; } = null;
        public string? Message { get; set; } = null;

        public HttpStatusCode StatusCode { get; set; }

        public T? Data { get; set; }
    }
}
