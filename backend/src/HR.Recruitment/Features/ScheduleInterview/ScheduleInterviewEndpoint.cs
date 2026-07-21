namespace HR.Recruitment.Features.ScheduleInterview;

using MediatR;
using HR.Common.Domain;
using HR.Recruitment.Application.Dtos.InterviewSchedule;

/// <summary>
/// Endpoint for scheduling an interview
/// </summary>
public static class ScheduleInterviewEndpoint
{
    public static async Task<IResult> Handle(
        Guid applicationId,
        ScheduleInterviewRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new ScheduleInterviewCommand(applicationId, request, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Created($"/recruitment/interviews/{result.Id}", ApiResponse<InterviewScheduleDto>.Created(result));
    }
}
