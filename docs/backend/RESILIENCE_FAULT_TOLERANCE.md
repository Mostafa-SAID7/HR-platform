# Resilience & Fault Tolerance Patterns

Comprehensive guide to building resilient microservices with circuit breakers, retries, bulkheads, and fallback mechanisms.

---

## Resilience Strategy

### Resilience Pyramid

```
┌─────────────────────────────────────────┐
│          Observability/Alerting         │  Detect failures
├─────────────────────────────────────────┤
│      Bulkhead Isolation/Timeouts        │  Prevent cascading
├─────────────────────────────────────────┤
│    Circuit Breaker / Fallback Strategies
├─────────────────────────────────────────┤
│        Retry with Exponential Backoff   │  Recover from transient failures
├─────────────────────────────────────────┤
│   Health Checks & Service Discovery    │  Know service state
└─────────────────────────────────────────┘
```

---

## Circuit Breaker Pattern

### States

```
CLOSED (Normal)
├─ Requests flow through
├─ Failures tracked
└─ Threshold hit → Open

OPEN (Failing)
├─ Requests blocked immediately
├─ Quick failure (no remote call)
└─ Wait timeout → Half-Open

HALF-OPEN (Testing)
├─ Allow single test request
├─ Success → Close
└─ Failure → Open
```

### Implementation with Polly

```csharp
// Resilience/CircuitBreakerPolicies.cs
public static class CircuitBreakerPolicies
{
    // Define policies
    public static IAsyncPolicy<HttpResponseMessage> GetEmployeeServicePolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()  // 5xx or 408
            .OrResult(r => r.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .CircuitBreakerAsync<HttpResponseMessage>(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, timespan) =>
                {
                    Console.WriteLine($"Circuit breaker opened for {timespan.TotalSeconds}s");
                },
                onReset: () =>
                {
                    Console.WriteLine($"Circuit breaker reset");
                },
                onHalfOpen: () =>
                {
                    Console.WriteLine($"Circuit breaker half-open - testing...");
                }
            );
    }
}

// Register in DI
services.AddHttpClient("EmployeeService")
    .AddPolicyHandler(CircuitBreakerPolicies.GetEmployeeServicePolicy());

// Usage
var client = httpClientFactory.CreateClient("EmployeeService");
try
{
    var response = await client.GetAsync("https://employee-service/api/employees/123");
}
catch (BrokenCircuitException)
{
    Console.WriteLine("Service unavailable - circuit is open");
    // Use fallback
}
```

---

## Retry Pattern

### Exponential Backoff

```
Attempt 1: Immediate
Attempt 2: Wait 100ms
Attempt 3: Wait 200ms
Attempt 4: Wait 400ms
Attempt 5: Wait 800ms

Total time: ~1.5 seconds before giving up
```

```csharp
// Retry with exponential backoff
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(
        retryCount: 4,
        sleepDurationProvider: attempt =>
            TimeSpan.FromMilliseconds(Math.Pow(2, attempt) * 100),  // 100ms * 2^attempt
        onRetry: (outcome, timespan, retryCount, context) =>
        {
            Console.WriteLine(
                $"Retry #{retryCount} after {timespan.TotalMilliseconds}ms. " +
                $"Reason: {outcome.Result?.StatusCode}"
            );
        }
    );

// With jitter (prevent thundering herd)
var retryWithJitter = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(
        retryCount: 4,
        sleepDurationProvider: attempt =>
        {
            var exponentialWait = TimeSpan.FromMilliseconds(Math.Pow(2, attempt) * 100);
            var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(0, 100));
            return exponentialWait.Add(jitter);
        }
    );

// Register
services.AddHttpClient("PayrollService")
    .AddPolicyHandler(retryWithJitter);
```

### Retry Limits

```csharp
// Per-request retry limits
public class RetryLimitService
{
    private readonly ConcurrentDictionary<string, int> _retryCount = new();
    
    public bool CanRetry(string requestId, int maxRetries = 3)
    {
        var count = _retryCount.AddOrUpdate(requestId, 1, (key, old) => old + 1);
        
        if (count >= maxRetries)
        {
            _retryCount.TryRemove(requestId, out _);
            return false;
        }
        
        return true;
    }
}
```

---

## Bulkhead Isolation Pattern

### Limit Concurrent Requests

```
Without Bulkhead:
All requests to Employee Service compete for same thread pool
→ One slow operation blocks all others

With Bulkhead:
Employee Service gets 50 threads
Analytics Service gets 30 threads
Payroll Service gets 20 threads
→ One service's slowness doesn't affect others
```

```csharp
// Bulkhead Policy
var bulkheadPolicy = Policy.BulkheadAsync<HttpResponseMessage>(
    maxParallelization: 50,    // Max 50 concurrent requests
    maxQueuingActions: 100,    // Max 100 waiting in queue
    onBulkheadRejectedAsync: context =>
    {
        Console.WriteLine("Bulkhead rejected - too many concurrent requests");
        return Task.CompletedTask;
    }
);

// Wrap with timeout
var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(5));
var combinedPolicy = Policy.WrapAsync(bulkheadPolicy, timeoutPolicy);

services.AddHttpClient("EmployeeService")
    .AddPolicyHandler(combinedPolicy);

// Per-service bulkheads
services.AddHttpClient("EmployeeService")
    .AddPolicyHandler(BulkheadPolicies.CreatePolicy("Employee", 50, 100));

services.AddHttpClient("AnalyticsService")
    .AddPolicyHandler(BulkheadPolicies.CreatePolicy("Analytics", 30, 50));

services.AddHttpClient("PayrollService")
    .AddPolicyHandler(BulkheadPolicies.CreatePolicy("Payroll", 20, 30));
```

---

## Timeout Pattern

### Request Timeout

```csharp
// Global timeout
var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(
    TimeSpan.FromSeconds(5),
    onTimeoutAsync: (context, timespan, task) =>
    {
        Console.WriteLine($"Request timed out after {timespan.TotalSeconds}s");
        return Task.CompletedTask;
    }
);

// Per-service timeout
services.AddHttpClient("EmployeeService")
    .ConfigureHttpClient(client => client.Timeout = TimeSpan.FromSeconds(5))
    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(5)));

// gRPC timeout
var response = await grpcClient.GetEmployeeAsync(
    request,
    deadline: DateTime.UtcNow.AddSeconds(5)
);
```

### Timeout Hierarchy

```csharp
// API Gateway timeout: 30s (allow time for all services)
// Service timeout: 15s (reserve time for retries)
// Database timeout: 5s (quick fail)
// Cache timeout: 1s (quick fallback)

public class TimeoutConfiguration
{
    public const int GatewayTimeoutSeconds = 30;
    public const int ServiceTimeoutSeconds = 15;
    public const int DatabaseTimeoutSeconds = 5;
    public const int CacheTimeoutSeconds = 1;
}
```

---

## Fallback Strategy

### Fallback Types

```csharp
// Type 1: Cache Fallback
public class EmployeeGrpcClientWithFallback
{
    private readonly EmployeeService.EmployeeServiceClient _client;
    private readonly IDistributedCache _cache;
    
    public async Task<EmployeeResponse> GetEmployeeAsync(string employeeId)
    {
        try
        {
            var response = await _client.GetEmployeeAsync(
                new GetEmployeeRequest { EmployeeId = employeeId },
                deadline: DateTime.UtcNow.AddSeconds(5)
            );
            
            // Update cache with fresh data
            await _cache.SetStringAsync(
                $"employee:{employeeId}",
                JsonSerializer.Serialize(response),
                new DistributedCacheEntryOptions 
                { 
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                }
            );
            
            return response;
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
        {
            // Fall back to cache (may be stale)
            var cached = await _cache.GetStringAsync($"employee:{employeeId}");
            if (cached != null)
            {
                return JsonSerializer.Deserialize<EmployeeResponse>(cached);
            }
            throw;
        }
    }
}

// Type 2: Default/Stub Fallback
public class PayrollServiceWithFallback
{
    private readonly IPayrollGrpcClient _payrollClient;
    private readonly ILogger<PayrollServiceWithFallback> _logger;
    
    public async Task<PayrollResponse> GetPayrollAsync(string employeeId)
    {
        try
        {
            return await _payrollClient.GetPayrollAsync(employeeId);
        }
        catch (ServiceUnavailableException)
        {
            _logger.LogWarning($"Payroll service unavailable for {employeeId}");
            
            // Return default/stub response
            return new PayrollResponse
            {
                EmployeeId = employeeId,
                BaseSalary = 0,
                LastCalculatedDate = DateTime.MinValue,
                IsStub = true,
                Message = "Using fallback data - service unavailable"
            };
        }
    }
}

// Type 3: Async Queue Fallback
public class NotificationServiceWithFallback
{
    private readonly INotificationClient _notificationClient;
    private readonly IBackgroundJobQueue _jobQueue;
    
    public async Task SendAsync(Notification notification)
    {
        try
        {
            await _notificationClient.SendAsync(notification);
        }
        catch (ServiceUnavailableException)
        {
            // Queue for retry later
            await _jobQueue.QueueAsync(
                new SendNotificationJob 
                { 
                    Notification = notification,
                    RetryCount = 0
                }
            );
        }
    }
}
```

---

## Adaptive Resilience

### Self-Healing with Metrics

```csharp
// Monitor service health and adapt
public class AdaptiveResilienceService
{
    private readonly IMetricsCollector _metrics;
    private readonly IResiliencyPolicyStore _policies;
    private readonly ILogger<AdaptiveResilienceService> _logger;
    
    public async Task EvaluateAndUpdatePoliciesAsync()
    {
        while (true)
        {
            // Check Employee Service health
            var employeeMetrics = await _metrics.GetServiceMetricsAsync("employee-service");
            
            // If error rate > 10%, increase circuit breaker threshold
            if (employeeMetrics.ErrorRate > 0.10)
            {
                _logger.LogWarning(
                    $"High error rate for employee-service ({employeeMetrics.ErrorRate:P}). " +
                    "Increasing circuit breaker sensitivity."
                );
                
                await _policies.UpdateCircuitBreakerAsync(
                    "employee-service",
                    handledEventsAllowedBeforeBreaking: 3  // Reduced from 5
                );
            }
            
            // If latency > 1s, reduce concurrent request limit
            if (employeeMetrics.AverageLatency > 1000)
            {
                _logger.LogWarning(
                    $"High latency for employee-service ({employeeMetrics.AverageLatency}ms). " +
                    "Reducing bulkhead limit."
                );
                
                await _policies.UpdateBulkheadAsync(
                    "employee-service",
                    maxParallelization: 30  // Reduced from 50
                );
            }
            
            // If recovering, relax policies
            if (employeeMetrics.ErrorRate < 0.01 && employeeMetrics.AverageLatency < 200)
            {
                _logger.LogInformation(
                    "employee-service recovered. Restoring normal policies."
                );
                
                await _policies.ResetToDefaultAsync("employee-service");
            }
            
            await Task.Delay(TimeSpan.FromMinutes(1));
        }
    }
}
```

---

## Testing Resilience

### Chaos Testing

```csharp
// Simulate failures to test resilience
public class ChaosTestingService
{
    private readonly Random _random = new();
    
    // Randomly fail requests for testing
    public async Task<T> ExecuteWithChaosAsync<T>(Func<Task<T>> operation, double failureRate = 0.1)
    {
        if (_random.NextDouble() < failureRate)
        {
            throw new SimulatedFailureException("Chaos: Simulated failure");
        }
        
        return await operation();
    }
    
    // Simulate latency
    public async Task<T> ExecuteWithLatencyAsync<T>(
        Func<Task<T>> operation, 
        int minDelayMs = 100, 
        int maxDelayMs = 5000)
    {
        var delay = _random.Next(minDelayMs, maxDelayMs);
        await Task.Delay(delay);
        return await operation();
    }
}

// Usage in integration tests
[TestFixture]
public class PayrollServiceResilienceTests
{
    private readonly ChaosTestingService _chaos = new();
    
    [Test]
    public async Task ShouldRetryOnTransientFailure()
    {
        // Simulate transient failure
        var attempt = 0;
        var response = await _chaos.ExecuteWithChaosAsync(
            async () =>
            {
                attempt++;
                if (attempt < 3)
                    throw new HttpRequestException("Transient");
                
                return new PayrollResponse { BaseSalary = 50000 };
            },
            failureRate: 1.0
        );
        
        Assert.That(attempt, Is.EqualTo(3));
        Assert.That(response.BaseSalary, Is.EqualTo(50000));
    }
    
    [Test]
    public async Task ShouldUseFallbackOnPersistentFailure()
    {
        // Simulate persistent failure
        Func<Task<PayrollResponse>> operation = async () =>
        {
            throw new ServiceUnavailableException();
        };
        
        var result = await _payrollService.GetPayrollWithFallbackAsync("EMP123");
        
        Assert.That(result.IsStub, Is.True);
    }
}
```

---

## Monitoring Resilience

```csharp
// Metrics for resilience
public class ResilienceMetrics
{
    public string ServiceName { get; set; }
    
    // Circuit breaker
    public int CircuitBreakerOpenCount { get; set; }
    public int CircuitBreakerHalfOpenCount { get; set; }
    
    // Retries
    public int RetryAttempts { get; set; }
    public int RetrySuccesses { get; set; }
    public int RetryFailures { get; set; }
    
    // Bulkhead
    public int BulkheadRejections { get; set; }
    public int AverageConcurrentRequests { get; set; }
    
    // Timeout
    public int TimeoutOccurrences { get; set; }
    
    // Fallback
    public int FallbackInvocations { get; set; }
    public int CacheFallbackHits { get; set; }
}

// Alert on resilience issues
public class ResilienceAlerts
{
    // Alert if circuit breaker opens
    - alert: CircuitBreakerOpen
      expr: circuit_breaker_open_count > 0
      for: 2m
      annotations:
        summary: "Circuit breaker opened for {{ $labels.service }}"
    
    // Alert if retry rate too high
    - alert: HighRetryRate
      expr: (retry_failures / (retry_attempts)) > 0.5
      for: 5m
      annotations:
        summary: "High retry rate for {{ $labels.service }}"
    
    // Alert if bulkhead rejecting
    - alert: BulkheadRejections
      expr: bulkhead_rejections > 10
      for: 1m
      annotations:
        summary: "Bulkhead rejecting requests for {{ $labels.service }}"
}
```

---

**Last Updated:** July 2026
**Status:** Resilience Patterns Complete
