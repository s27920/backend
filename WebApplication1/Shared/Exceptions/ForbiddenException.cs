namespace WebApplication1.Shared.Exceptions;

public class ForbiddenException : AppException
{
    public ForbiddenException(string message = "Forbidden")
        : base(message, 403) { }
}