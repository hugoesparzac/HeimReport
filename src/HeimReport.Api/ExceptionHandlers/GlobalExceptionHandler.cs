using HeimReport.Api.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HeimReport.Api.ExceptionHandlers;

public partial class GlobalExceptionHandler(IWebHostEnvironment env, ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = new ProblemDetails
        {
            Instance = httpContext.Request.Path,
            Extensions = { ["traceId"] = httpContext.TraceIdentifier }
        };

        switch (exception)
        {
            case FluentValidation.ValidationException fluentException:
                LogValidationFailed(httpContext.Request.Path, string.Join("; ", fluentException.Errors.Select(e => e.ErrorMessage)));
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Validation Error";
                problemDetails.Detail = "One or more validation errors occurred.";
                problemDetails.Extensions["errors"] = fluentException.Errors
                    .GroupBy(x => x.PropertyName, x => x.ErrorMessage)
                    .ToDictionary(g => g.Key, g => g.ToArray());
                break;

            case DbUpdateConcurrencyException concurrencyException:
                LogConcurrencyConflict(concurrencyException, httpContext.Request.Path);
                problemDetails.Status = StatusCodes.Status409Conflict;
                problemDetails.Title = "Concurrency Conflict";
                problemDetails.Detail = "The record was modified by another process. Please reload and try again.";
                break;

            case DbUpdateException dbUpdateException when dbUpdateException.InnerException is PostgresException { SqlState: "23505" } pgEx:
                LogUniqueConstraintViolation(pgEx, httpContext.Request.Path);
                problemDetails.Status = StatusCodes.Status409Conflict;
                problemDetails.Title = "Data Conflict";
                problemDetails.Detail = "A record with the same unique identifier already exists. Please verify fields like Email, Username, or National ID.";
                break;

            case DbUpdateException dbUpdateException:
                LogUnhandledDatabaseError(dbUpdateException, httpContext.Request.Path);
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Title = "Database Error";
                problemDetails.Detail = env.IsDevelopment()
                    ? dbUpdateException.InnerException?.Message ?? dbUpdateException.Message
                    : "An internal database error occurred.";
                break;

            case InvalidOperationException invalidOperationException:
                LogInvalidOperation(invalidOperationException, httpContext.Request.Path);
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Title = "Internal Server Error";
                problemDetails.Detail = env.IsDevelopment()
                    ? invalidOperationException.Message
                    : "An internal error has occurred on the server.";
                break;

            case UnauthorizedAccessException unauthorizedAccessException:
                LogUnauthorizedAccess(unauthorizedAccessException, httpContext.Request.Path);
                problemDetails.Status = StatusCodes.Status401Unauthorized;
                problemDetails.Title = "Unauthorized";
                problemDetails.Detail = "You do not have permission to access this resource.";
                break;

            case NotFoundException notFoundException:
                LogResourceNotFound(notFoundException.Message);
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Title = "Resource Not Found";
                problemDetails.Detail = notFoundException.Message;
                break;

            case DomainException domainException:
                LogDomainRuleViolated(httpContext.Request.Path, domainException.Message);
                problemDetails.Status = domainException.StatusCode;
                problemDetails.Title = domainException.StatusCode == StatusCodes.Status409Conflict
                    ? "Conflict"
                    : "Business Rule Violation";
                problemDetails.Detail = domainException.Message;
                break;

            case OperationCanceledException:
                LogRequestCancelled(httpContext.Request.Path);
                return true;

            default:
                LogUnhandledException(exception, httpContext.Request.Path);
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Title = "Internal Server Error";
                problemDetails.Detail = env.IsDevelopment() ? exception.Message : "An internal error has occurred on the server.";
                break;
        }

        httpContext.Response.ContentType = "application/problem+json";
        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Validation failed for {Path}: {Errors}")]
    private partial void LogValidationFailed(PathString path, string errors);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Concurrency conflict on {Path}")]
    private partial void LogConcurrencyConflict(Exception ex, PathString path);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Unique constraint violation on {Path}")]
    private partial void LogUniqueConstraintViolation(Exception ex, PathString path);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled database error on {Path}")]
    private partial void LogUnhandledDatabaseError(Exception ex, PathString path);

    [LoggerMessage(Level = LogLevel.Error, Message = "Invalid operation on {Path}")]
    private partial void LogInvalidOperation(Exception ex, PathString path);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Unauthorized access attempt on {Path}")]
    private partial void LogUnauthorizedAccess(Exception ex, PathString path);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Resource not found: {Message}")]
    private partial void LogResourceNotFound(string message);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Domain rule violated on {Path}: {Message}")]
    private partial void LogDomainRuleViolated(PathString path, string message);

    [LoggerMessage(Level = LogLevel.Information, Message = "Request was cancelled by the client on {Path}")]
    private partial void LogRequestCancelled(PathString path);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception on {Path}")]
    private partial void LogUnhandledException(Exception ex, PathString path);
}
