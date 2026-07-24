using Microsoft.AspNetCore.Http;

namespace HeimReport.Api.Exceptions;

public class DomainException : Exception
{
    public int StatusCode { get; } = StatusCodes.Status400BadRequest;

    public DomainException()
    {
    }

    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public DomainException(string message, int statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public DomainException(string message, Exception innerException, int statusCode) : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    public static DomainException EntityInUse(string entityName, string reason)
        => new($"Cannot delete {entityName} because {reason}.", StatusCodes.Status409Conflict);

    public static DomainException AlreadyExists(string entityName, string field, object value)
        => new($"A {entityName} with {field} '{value}' already exists.", StatusCodes.Status409Conflict);

    public static DomainException InvalidStateTransition(string entityName, string fromState, string toState)
        => new($"Cannot transition {entityName} from '{fromState}' to '{toState}'.");
}