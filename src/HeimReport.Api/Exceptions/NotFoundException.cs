namespace HeimReport.Api.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException()
    {
    }

    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public static NotFoundException ForEntity<T>(object key)
        => new($"{typeof(T).Name} with Id '{key}' was not found.");
}