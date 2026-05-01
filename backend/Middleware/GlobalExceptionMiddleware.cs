using backend.Application.Exceptions;
using System.Text.Json;

namespace backend.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (BadRequestException ex)
        {
            _logger.LogWarning(ex,
                "Bad request for {Method} {Path}. CorrelationId={CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier);

            await WriteProblemDetailsAsync(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex,
                "Unauthorized request for {Method} {Path}. CorrelationId={CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier);

            await WriteProblemDetailsAsync(context, StatusCodes.Status401Unauthorized, ex.Message);
        }
        catch (ForbiddenException ex)
        {
            _logger.LogWarning(ex,
                "Forbidden request for {Method} {Path}. CorrelationId={CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier);

            await WriteProblemDetailsAsync(context, StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (NotFoundException ex)
        {
            _logger.LogInformation(ex,
                "Resource not found for {Method} {Path}. CorrelationId={CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier);

            await WriteProblemDetailsAsync(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (ConflictException ex)
        {
            _logger.LogWarning(ex,
                "Conflict for {Method} {Path}. CorrelationId={CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier);

            await WriteProblemDetailsAsync(context, StatusCodes.Status409Conflict, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex,
                "Business validation failure for {Method} {Path}. CorrelationId={CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier);

            await WriteProblemDetailsAsync(context, StatusCodes.Status409Conflict, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unhandled exception for {Method} {Path}. CorrelationId={CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier);

            await WriteProblemDetailsAsync(context, StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.");
        }
    }

    private static Task WriteProblemDetailsAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var payload = new
        {
            message,
            correlationId = context.TraceIdentifier
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
