namespace talaria_links_api.Common.Exceptions
{
    public abstract class AppException(string message, int statusCode) : Exception
    {
        public int StatusCode { get; } = statusCode;
    }

    public sealed class ConflictException(string message) : 
        AppException(message, StatusCodes.Status409Conflict);

    public sealed class UnauthorizedException(string message) :
        AppException(message, StatusCodes.Status401Unauthorized);

    public sealed class NotFoundException(string message) : 
        AppException(message, StatusCodes.Status404NotFound);
}
