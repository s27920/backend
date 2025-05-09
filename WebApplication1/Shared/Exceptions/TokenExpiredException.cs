namespace WebApplication1.Shared.Exceptions;

public class TokenExpiredException : AppException
{
    public TokenExpiredException(string message = "The token has expired.")
        : base(message, 401) { }
}