namespace HR.Employee.Features.TerminateEmployee;

using MediatR;
using HR.Common.Domain;
using HR.Employee.Application.Dtos.Employee;

/// <summary>
/// Endpoint for terminating an employee
/// </summary>
public static class TerminateEmployeeEndpoint
{
    public static async Task<IResult> Handle(
        Guid id,
        TerminateEmployeeRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new TerminateEmployeeCommand(id, request.TerminationDate, tenantId);
        await mediator.Send(command, cancellationToken);
        return Results.Ok(ApiResponse.Ok("Employee terminated successfully"));
    }
}
