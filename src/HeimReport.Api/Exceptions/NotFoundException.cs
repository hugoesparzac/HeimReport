namespace HeimReport.Api.Exceptions;

public class NotFoundException(string message) : Exception(message)
{
    public static NotFoundException ForEntity<T>(object key) 
        => new($"{typeof(T).Name} with Id '{key}' was not found.");
}