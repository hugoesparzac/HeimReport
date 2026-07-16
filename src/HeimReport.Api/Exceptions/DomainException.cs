namespace HeimReport.Api.Exceptions;

public class DomainException : Exception
{
    public DomainException()
    {
    }

    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public static DomainException EntityInUse(string entityName, string reason)
        => new($"Cannot delete {entityName} because {reason}.");

    public static DomainException AlreadyExists(string entityName, string field, object value)
        => new($"A {entityName} with {field} '{value}' already exists.");

    public static DomainException InvalidStateTransition(
        string entityName,
        string fromState,
        string toState)
        => new($"Cannot transition {entityName} from '{fromState}' to '{toState}'.");
}