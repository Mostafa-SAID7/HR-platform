namespace HR.Performance.Configuration;

using HR.Performance.Features.CreatePerformanceReview;
using HR.Performance.Features.GetPerformanceReviews;
using HR.Performance.Features.GetPerformanceReviewById;
using HR.Performance.Features.SubmitPerformanceReview;
using HR.Performance.Features.ApprovePerformanceReview;
using HR.Performance.Features.SetPerformanceRatings;
using HR.Performance.Features.AddPerformanceFeedback;

/// <summary>
/// API route configuration for Performance service
/// Organized by feature following SOLID principles
/// </summary>
public static class RouteConfiguration
{
    /// <summary>
    /// Map all API endpoints
    /// </summary>
    public static WebApplication MapPerformanceRoutes(this WebApplication app)
    {
        var apiGroup = app.MapGroup("/performance")
            .WithTags("Performance Reviews");

        // Performance review endpoints
        MapPerformanceReviewRoutes(apiGroup);

        return app;
    }

    /// <summary>
    /// Map performance review endpoints
    /// </summary>
    private static void MapPerformanceReviewRoutes(RouteGroupBuilder apiGroup)
    {
        apiGroup.MapPost("/reviews", CreatePerformanceReviewEndpoint.Handle)
            .WithName("CreatePerformanceReview")
            .WithOpenApi()
            .RequireAuthorization();

        apiGroup.MapGet("/reviews", GetPerformanceReviewsEndpoint.Handle)
            .WithName("GetPerformanceReviews")
            .WithOpenApi()
            .RequireAuthorization();

        apiGroup.MapGet("/reviews/{id:guid}", GetPerformanceReviewByIdEndpoint.Handle)
            .WithName("GetPerformanceReviewById")
            .WithOpenApi()
            .RequireAuthorization();

        apiGroup.MapPost("/reviews/{id:guid}/submit", SubmitPerformanceReviewEndpoint.Handle)
            .WithName("SubmitPerformanceReview")
            .WithOpenApi()
            .RequireAuthorization();

        apiGroup.MapPost("/reviews/{id:guid}/approve", ApprovePerformanceReviewEndpoint.Handle)
            .WithName("ApprovePerformanceReview")
            .WithOpenApi()
            .RequireAuthorization();

        apiGroup.MapPost("/reviews/{id:guid}/ratings", SetPerformanceRatingsEndpoint.Handle)
            .WithName("SetPerformanceRatings")
            .WithOpenApi()
            .RequireAuthorization();

        apiGroup.MapPost("/reviews/{id:guid}/feedback", AddPerformanceFeedbackEndpoint.Handle)
            .WithName("AddPerformanceFeedback")
            .WithOpenApi()
            .RequireAuthorization();
    }
}
