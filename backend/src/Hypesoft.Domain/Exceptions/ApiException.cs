using System.Net;

namespace Hypesoft.Domain.Exceptions;

public class ApiException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public ApiException(
        string message,
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError
    )
        : base(message)
    {
        StatusCode = statusCode;
    }

    public ApiException(
        string message,
        Exception innerException,
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError
    )
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}
