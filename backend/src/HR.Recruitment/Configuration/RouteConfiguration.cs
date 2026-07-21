namespace HR.Recruitment.Configuration;

using HR.Recruitment.Features.CreateJobPosting;
using HR.Recruitment.Features.PublishJobPosting;
using HR.Recruitment.Features.GetJobPostings;
using HR.Recruitment.Features.ApplyJob;
using HR.Recruitment.Features.GetApplications;
using HR.Recruitment.Features.ScheduleInterview;
using HR.Recruitment.Features.CreateOfferLetter;

/// <summary>
/// API route configuration for Recruitment service
/// Organized by feature following SOLID principles
/// </summary>
public static class RouteConfiguration
{
    /// <summary>
    /// Map all API endpoints
    /// </summary>
    public static WebApplication MapRecruitmentRoutes(this WebApplication app)
    {
        var apiGroup = app.MapGroup("/recruitment")
            .WithTags("Recruitment");

        // Job Postings routes
        MapJobPostingRoutes(apiGroup);

        // Job Applications routes
        MapJobApplicationRoutes(apiGroup);

        // Interview routes
        MapInterviewRoutes(apiGroup);

        // Offer Letter routes
        MapOfferLetterRoutes(apiGroup);

        return app;
    }

    /// <summary>
    /// Map job posting endpoints
    /// </summary>
    private static void MapJobPostingRoutes(RouteGroupBuilder apiGroup)
    {
        apiGroup.MapPost("/job-postings", CreateJobPostingEndpoint.Handle)
            .WithName("CreateJobPosting")
            .WithOpenApi()
            .RequireAuthorization();

        apiGroup.MapGet("/job-postings", GetJobPostingsEndpoint.Handle)
            .WithName("GetJobPostings")
            .WithOpenApi();

        apiGroup.MapPost("/job-postings/{id:guid}/publish", PublishJobPostingEndpoint.Handle)
            .WithName("PublishJobPosting")
            .WithOpenApi()
            .RequireAuthorization();
    }

    /// <summary>
    /// Map job application endpoints
    /// </summary>
    private static void MapJobApplicationRoutes(RouteGroupBuilder apiGroup)
    {
        apiGroup.MapPost("/job-postings/{jobPostingId:guid}/apply", ApplyJobEndpoint.Handle)
            .WithName("ApplyJob")
            .WithOpenApi();

        apiGroup.MapGet("/job-postings/{jobPostingId:guid}/applications", GetApplicationsEndpoint.Handle)
            .WithName("GetApplications")
            .WithOpenApi()
            .RequireAuthorization();
    }

    /// <summary>
    /// Map interview endpoints
    /// </summary>
    private static void MapInterviewRoutes(RouteGroupBuilder apiGroup)
    {
        apiGroup.MapPost("/applications/{applicationId:guid}/schedule-interview", ScheduleInterviewEndpoint.Handle)
            .WithName("ScheduleInterview")
            .WithOpenApi()
            .RequireAuthorization();
    }

    /// <summary>
    /// Map offer letter endpoints
    /// </summary>
    private static void MapOfferLetterRoutes(RouteGroupBuilder apiGroup)
    {
        apiGroup.MapPost("/offer-letters", CreateOfferLetterEndpoint.Handle)
            .WithName("CreateOfferLetter")
            .WithOpenApi()
            .RequireAuthorization();
    }
}
