using HeimReport.Api.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HeimReport.Api.ExceptionHandlers;

public class GlobalExceptionHandler(IWebHostEnvironment env, ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
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
                logger.LogWarning("Validation failed for {Path}: {Errors}",
                    httpContext.Request.Path,
                    string.Join("; ", fluentException.Errors.Select(e => e.ErrorMessage)));
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Validation Error";
                problemDetails.Detail = "One or more validation errors occurred.";
                problemDetails.Extensions["errors"] = fluentException.Errors
                    .GroupBy(x => x.PropertyName, x => x.ErrorMessage)
                    .ToDictionary(g => g.Key, g => g.ToArray());
                break;

            case DbUpdateConcurrencyException concurrencyException:
                logger.LogWarning(concurrencyException, "Concurrency conflict on {Path}", httpContext.Request.Path);
                problemDetails.Status = StatusCodes.Status409Conflict;
                problemDetails.Title = "Concurrency Conflict";
                problemDetails.Detail = "The record was modified by another process. Please reload and try again.";
                break;

            case DbUpdateException dbUpdateException when dbUpdateException.InnerException is PostgresException { SqlState: "23505" } pgEx:
                logger.LogWarning(pgEx, "Unique constraint violation on {Path}", httpContext.Request.Path);
                problemDetails.Status = StatusCodes.Status409Conflict;
                problemDetails.Title = "Data Conflict";
                problemDetails.Detail = "A record with the same unique identifier already exists. Please verify fields like Email, Username, or National ID.";
                break;

            case DbUpdateException dbUpdateException:
                logger.LogError(dbUpdateException, "Unhandled database error on {Path}", httpContext.Request.Path);
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Title = "Database Error";
                problemDetails.Detail = env.IsDevelopment()
                    ? dbUpdateException.InnerException?.Message ?? dbUpdateException.Message
                    : "An internal database error occurred.";
                break;

            case InvalidOperationException invalidOperationException:
                logger.LogWarning(invalidOperationException, "Bad request on {Path}", httpContext.Request.Path);
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Bad Request";
                problemDetails.Detail = invalidOperationException.Message;
                break;

            case UnauthorizedAccessException unauthorizedAccessException:
                logger.LogWarning(unauthorizedAccessException, "Unauthorized access attempt on {Path}", httpContext.Request.Path);
                problemDetails.Status = StatusCodes.Status401Unauthorized;
                problemDetails.Title = "Unauthorized";
                problemDetails.Detail = "You do not have permission to access this resource.";
                break;

            case NotFoundException notFoundException:
                logger.LogWarning("Resource not found: {Message}", notFoundException.Message);
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Title = "Resource Not Found";
                problemDetails.Detail = notFoundException.Message;
                break;

            case OperationCanceledException:
                logger.LogInformation("Request was cancelled by the client on {Path}", httpContext.Request.Path);
                return true;

            default:
                logger.LogError(exception, "Unhandled exception on {Path}", httpContext.Request.Path);
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
}