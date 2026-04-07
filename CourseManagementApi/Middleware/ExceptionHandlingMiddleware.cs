using System.Net;
using System.Text.Json;
using CourseManagementApi.Exceptions;

namespace CourseManagementApi.Middleware;

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (status, message) = exception switch
        {
            NotFoundException nf => (HttpStatusCode.NotFound, nf.Message),
            ForbiddenException fb => (HttpStatusCode.Forbidden, fb.Message),
            BadRequestException br => (HttpStatusCode.BadRequest, br.Message),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;

        var body = JsonSerializer.Serialize(new
        {
            status = (int)status,
            error = status.ToString(),
            message
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        return context.Response.WriteAsync(body);
    }
}
