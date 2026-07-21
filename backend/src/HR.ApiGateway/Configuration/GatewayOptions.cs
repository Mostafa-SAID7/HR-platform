namespace HR.ApiGateway.Configuration;

/// <summary>
/// Configuration options for the API Gateway.
/// </summary>
public class GatewayOptions
{
    public const string SectionName = "Gateway";

    /// <summary>
    /// Port the gateway listens on.
    /// </summary>
    public int Port { get; set; } = 5000;

    /// <summary>
    /// Service endpoints configuration.
    /// </summary>
    public ServiceEndpointsOptions ServiceEndpoints { get; set; } = new();

    /// <summary>
    /// Rate limiting configuration.
    /// </summary>
    public RateLimitOptions RateLimit { get; set; } = new();

    /// <summary>
    /// Authentication configuration.
    /// </summary>
    public AuthenticationOptions Authentication { get; set; } = new();
}

/// <summary>
/// Service endpoints configuration.
/// </summary>
public class ServiceEndpointsOptions
{
    public string IdentityService { get; set; } = "http://localhost:5001";
    public string EmployeeService { get; set; } = "http://localhost:5002";
    public string PerformanceService { get; set; } = "http://localhost:5003";
    public string RecruitmentService { get; set; } = "http://localhost:5004";
    public string AttendanceService { get; set; } = "http://localhost:5005";
    public string PayrollService { get; set; } = "http://localhost:5006";
    public string AnalyticsService { get; set; } = "http://localhost:5007";
    public string NotificationService { get; set; } = "http://localhost:5008";
    public string AuditService { get; set; } = "http://localhost:5009";
}

/// <summary>
/// Rate limiting configuration.
/// </summary>
public class RateLimitOptions
{
    /// <summary>
    /// Enable rate limiting.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Requests per minute per user.
    /// </summary>
    public int RequestsPerMinute { get; set; } = 60;

    /// <summary>
    /// Requests per hour per user.
    /// </summary>
    public int RequestsPerHour { get; set; } = 1000;

    /// <summary>
    /// HTTP status code for rate limit exceeded.
    /// </summary>
    public int HttpStatusCode { get; set; } = 429;
}

/// <summary>
/// Authentication configuration.
/// </summary>
public class AuthenticationOptions
{
    /// <summary>
    /// JWT token issuer.
    /// </summary>
    public string JwtIssuer { get; set; } = "https://identityservice";

    /// <summary>
    /// JWT token audience.
    /// </summary>
    public string JwtAudience { get; set; } = "hranalytics";

    /// <summary>
    /// JWT token secret key.
    /// </summary>
    public string JwtSecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Public key for token validation.
    /// </summary>
    public string? JwtPublicKey { get; set; }

    /// <summary>
    /// Enable bearer token validation.
    /// </summary>
    public bool ValidateToken { get; set; } = true;

    /// <summary>
    /// Public routes that don't require authentication.
    /// </summary>
    public List<string> PublicRoutes { get; set; } = new()
    {
        "/health",
        "/health/ready",
        "/swagger",
        "/identity/login",
        "/identity/register",
        "/identity/refresh-token"
    };
}
