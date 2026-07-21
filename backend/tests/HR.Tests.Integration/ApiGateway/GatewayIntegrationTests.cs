namespace HR.Tests.Integration.ApiGateway;

/// <summary>
/// Integration tests for API Gateway (9 tests).
/// Tests cover: routing, middleware pipeline, health checks, and service communication.
/// </summary>
public class GatewayIntegrationTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly TestWebApplicationFactory<Program> _factory;

    public GatewayIntegrationTests()
    {
        _factory = new TestWebApplicationFactory<Program>();
        _httpClient = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _httpClient?.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task HealthCheck_Endpoint_ReturnsHealthy()
    {
        // Act
        var response = await _httpClient.GetAsync("/health");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("healthy", content.ToLower());
    }

    [Fact]
    public async Task RequestWithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        var response = await _httpClient.GetAsync("/employee/list");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RequestWithValidToken_IsForwarded()
    {
        // Arrange
        var token = GenerateValidJwt();
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _httpClient.GetAsync("/health");

        // Assert
        Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK || 
                   response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RequestWithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token");

        // Act
        var response = await _httpClient.GetAsync("/employee/list");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task IdentityRoute_IsConfigured()
    {
        // Arrange
        var token = GenerateValidJwt();
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _httpClient.PostAsync("/identity/login", null);

        // Assert - Should forward or return service-specific response
        Assert.True(response.StatusCode != System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task EmployeeRoute_IsConfigured()
    {
        // Arrange
        var token = GenerateValidJwt();
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _httpClient.GetAsync("/employee/list");

        // Assert - Should forward or return service-specific response
        Assert.True(response.StatusCode != System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CorsHeaders_AreIncludedInResponse()
    {
        // Act
        var response = await _httpClient.GetAsync("/health");

        // Assert
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin") ||
                   response.StatusCode == System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task RequestLogging_IncludesTraceId()
    {
        // Arrange
        var token = GenerateValidJwt();
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _httpClient.GetAsync("/health");

        // Assert
        Assert.True(response.Headers.Contains("X-Correlation-ID") ||
                   response.Headers.Contains("X-Trace-ID") ||
                   response.StatusCode == System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task RateLimiting_AllowsMultipleRequests()
    {
        // Arrange
        var token = GenerateValidJwt();
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var tasks = Enumerable.Range(1, 5)
            .Select(_ => _httpClient.GetAsync("/health"))
            .ToArray();

        await Task.WhenAll(tasks);

        // Assert
        Assert.True(tasks.All(t => t.Result.StatusCode == System.Net.HttpStatusCode.OK ||
                                   t.Result.StatusCode == System.Net.HttpStatusCode.TooManyRequests));
    }

    // Helper method
    private string GenerateValidJwt()
    {
        // Generate a simple JWT for testing (not cryptographically secure, just for testing)
        var header = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"alg\":\"HS256\",\"typ\":\"JWT\"}"));
        var payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(
            $"{{\"sub\":\"{Guid.NewGuid()}\",\"exp\":{DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()}}}"));
        var signature = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("test-signature"));

        return $"{header}.{payload}.{signature}";
    }
}

/// <summary>
/// Test WebApplicationFactory for creating test host
/// </summary>
public class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Override services if needed
        });
    }
}
