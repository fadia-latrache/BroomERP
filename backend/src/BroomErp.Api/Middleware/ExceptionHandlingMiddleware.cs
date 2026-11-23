using System.Text.Json;
using BroomErp.Domain.Exceptions;

namespace BroomErp.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (DomainException ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex.StatusCode;
            var response = new { detail = ex.Message };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled server error: {Message}", ex.Message);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            var response = new { detail = "An internal server error occurred." };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
