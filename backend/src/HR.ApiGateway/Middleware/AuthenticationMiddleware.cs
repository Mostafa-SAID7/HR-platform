namespace HR.ApiGateway.Middleware;

using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HR.ApiGateway.Configuration;

/// <summary>
/// Middleware for validating JWT tokens and adding user context to requests.
/// </summary>
public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationMiddleware> _logger;

    public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IOptionsMonitor<GatewayOptions> options)
    {
        var gatewayOptions = options.CurrentValue;
        var publicRoutes = gatewayOptions.Authentication.PublicRoutes;

        var currentPath = context.Request.Path.ToString().ToLower();
        var isPublicRoute = publicRoutes.Any(route => currentPath.StartsWith(route.ToLower()));

        if (isPublicRoute)
        {
            await _next(context);
            return;
        }

        var token = ExtractToken(context);

        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Missing token for protected route: {Path}", currentPath);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Unauthorized: Missing token" });
            return;
        }

        if (!ValidateToken(token, gatewayOptions))
        {
            _logger.LogWarning("Invalid token for route: {Path}", currentPath);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Unauthorized: Invalid token" });
            return;
        }

        var claims = ExtractClaims(token);
        context.Items["UserId"] = claims.ContainsKey("sub") ? claims["sub"] : null;
        context.Items["TenantId"] = claims.ContainsKey("tenant_id") ? claims["tenant_id"] : null;
        context.Items["Roles"] = claims.ContainsKey("role") ? claims["role"] : null;

        await _next(context);
    }

    private static string? ExtractToken(HttpContext context)
    {
        var authHeader = context.Request.Headers.Authorization.ToString();

        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return null;
        }

        return authHeader.Substring("Bearer ".Length).Trim();
    }

    private static bool ValidateToken(string token, GatewayOptions options)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(token))
            {
                return false;
            }

            var jwtToken = handler.ReadJwtToken(token);

            // Check expiration
            if (jwtToken.ValidTo < DateTime.UtcNow)
            {
                return false;
            }

            // Validate issuer
            if (!string.IsNullOrEmpty(options.Authentication.JwtIssuer) &&
                jwtToken.Issuer != options.Authentication.JwtIssuer)
            {
                return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static Dictionary<string, string> ExtractClaims(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            return jwtToken.Claims
                .ToDictionary(c => c.Type, c => c.Value);
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }
}

/// <summary>
/// Extension method to add authentication middleware.
/// </summary>
public static class AuthenticationMiddlewareExtensions
{
    public static IApplicationBuilder UseGatewayAuthentication(this IApplicationBuilder app)
    {
        return app.UseMiddleware<AuthenticationMiddleware>();
    }
}
