namespace HR.Common.Middleware;

using System.Net;
using Microsoft.AspNetCore.Http;
using Serilog;

/// <summary>
/// Global exception handling middleware.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.TraceIdentifier;
        context.Items["CorrelationId"] = correlationId;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unhandled exception with correlation ID {CorrelationId}", correlationId);
            await HandleExceptionAsync(context, ex, correlationId);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception, string correlationId)
    {
        context.Response.ContentType = "application/json";

        var response = new ApiResponse<object>();
        response.Error = new ErrorDetails { TraceId = correlationId };

        switch (exception)
        {
            case NotFoundException notFoundEx:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Success = false;
                response.Message = notFoundEx.Message;
                response.Error.Code = "NOT_FOUND";
                break;

            case HR.Common.Exceptions.ValidationException validationEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Success = false;
                response.Message = "Validation failed";
                response.Error.Code = "VALIDATION_ERROR";
                response.Error.Details = validationEx.Failures;
                break;

            case ForbiddenException forbiddenEx:
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                response.Success = false;
                response.Message = forbiddenEx.Message;
                response.Error.Code = "FORBIDDEN";
                break;

            case BusinessRuleViolationException businessEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Success = false;
                response.Message = businessEx.Message;
                response.Error.Code = "BUSINESS_RULE_VIOLATION";
                break;

            case AlreadyExistsException alreadyExistsEx:
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                response.Success = false;
                response.Message = alreadyExistsEx.Message;
                response.Error.Code = "ALREADY_EXISTS";
                break;

            case HR.Common.Exceptions.ConcurrencyException concurrencyEx:
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                response.Success = false;
                response.Message = concurrencyEx.Message;
                response.Error.Code = "CONCURRENCY_CONFLICT";
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Success = false;
                response.Message = "An internal server error occurred";
                response.Error.Code = "INTERNAL_SERVER_ERROR";
                break;
        }

        return context.Response.WriteAsJsonAsync(response);
    }
}

/// <summary>
/// Extension method to add exception handling middleware.
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
