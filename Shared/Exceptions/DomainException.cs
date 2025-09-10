using Shared.Errors;

namespace Shared.Exceptions;

public class DomainException : Exception
{
    public string ErrorCode { get; }
    public string ErrorMessage { get; }

    public DomainException(Error error)
        : base($"{error.Code}: {error.Message}")
    {
        ErrorCode = error.Code;
        ErrorMessage = error.Message;
    }

    public DomainException(string code, string message)
        : base($"{code}: {message}")
    {
        ErrorCode = code;
        ErrorMessage = message;
    }
}
