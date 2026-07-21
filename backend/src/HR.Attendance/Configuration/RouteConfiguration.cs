namespace HR.Attendance.Configuration;

using HR.Attendance.Features.CheckIn;
using HR.Attendance.Features.CheckOut;
using HR.Attendance.Features.RequestLeave;
using HR.Attendance.Features.GetLeaveRequests;
using HR.Attendance.Features.GetTodayAttendance;
using HR.Attendance.Features.ApproveLeave;

public static class RouteConfiguration
{
    public static WebApplication MapAttendanceRoutes(this WebApplication app)
    {
        var apiGroup = app.MapGroup("/attendance")
            .WithTags("Attendance");

        MapAttendanceRoutes(apiGroup);
        MapLeaveRoutes(apiGroup);

        return app;
    }

    private static void MapAttendanceRoutes(RouteGroupBuilder apiGroup)
    {
        apiGroup.MapPost("/check-in", CheckInEndpoint.Handle)
            .WithName("CheckIn")
            .WithOpenApi()
            .RequireAuthorization();

        apiGroup.MapPost("/check-out", CheckOutEndpoint.Handle)
            .WithName("CheckOut")
            .WithOpenApi()
            .RequireAuthorization();

        apiGroup.MapGet("/today", GetTodayAttendanceEndpoint.Handle)
            .WithName("GetTodayAttendance")
            .WithOpenApi()
            .RequireAuthorization();
    }

    private static void MapLeaveRoutes(RouteGroupBuilder apiGroup)
    {
        apiGroup.MapPost("/leave/request", RequestLeaveEndpoint.Handle)
            .WithName("RequestLeave")
            .WithOpenApi()
            .RequireAuthorization();

        apiGroup.MapGet("/leave/requests", GetLeaveRequestsEndpoint.Handle)
            .WithName("GetLeaveRequests")
            .WithOpenApi()
            .RequireAuthorization();

        apiGroup.MapPost("/leave/{id:guid}/approve", ApproveLeaveEndpoint.Handle)
            .WithName("ApproveLeave")
            .WithOpenApi()
            .RequireAuthorization();
    }
}
