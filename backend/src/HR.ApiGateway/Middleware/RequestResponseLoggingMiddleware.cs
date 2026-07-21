namespace HR.ApiGateway.Middleware;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

/// <summary>
/// Middleware for logging HTTP requests and responses.
/// </summary>
public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = context.TraceIdentifier;

        // Log request
        await LogRequestAsync(context, correlationId);

        // Copy the response stream to capture response body
        var originalBodyStream = context.Response.Body;
        using var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        try
        {
            await _next(context);

            stopwatch.Stop();

            // Log response
            await LogResponseAsync(context, correlationId, stopwatch.ElapsedMilliseconds);

            // Copy response back to original stream
            await responseStream.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Exception occurred during request processing. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private async Task LogRequestAsync(HttpContext context, string correlationId)
    {
        var request = context.Request;
        var body = string.Empty;

        // Only log body for POST/PUT/PATCH requests
        if (request.Method.ToUpper() is "POST" or "PUT" or "PATCH")
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            body = await reader.ReadToEndAsync();
            request.Body.Position = 0;

            if (body.Length > 1000)
            {
                body = body.Substring(0, 1000) + "... [truncated]";
            }
        }

        _logger.LogInformation(
            "HTTP Request - CorrelationId: {CorrelationId}, Method: {Method}, Path: {Path}, QueryString: {QueryString}, Body: {Body}",
            correlationId,
            request.Method,
            request.Path,
            request.QueryString,
            body);
    }

    private async Task LogResponseAsync(HttpContext context, string correlationId, long elapsedMilliseconds)
    {
        var response = context.Response;
        var body = string.Empty;

        if (response.Body.CanSeek)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(response.Body, Encoding.UTF8, leaveOpen: true);
            body = await reader.ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            if (body.Length > 1000)
            {
                body = body.Substring(0, 1000) + "... [truncated]";
            }
        }

        _logger.LogInformation(
            "HTTP Response - CorrelationId: {CorrelationId}, StatusCode: {StatusCode}, Duration: {Duration}ms, Body: {Body}",
            correlationId,
            response.StatusCode,
            elapsedMilliseconds,
            body);
    }
}

/// <summary>
/// Extension method to add request/response logging middleware.
/// </summary>
public static class RequestResponseLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestResponseLoggingMiddleware>();
    }
}
