namespace HR.Employee.Features.UpdateEmployee;

using MediatR;
using HR.Common.Domain;
using HR.Employee.Application.Dtos.Employee;

/// <summary>
/// Endpoint for updating an employee
/// </summary>
public static class UpdateEmployeeEndpoint
{
    public static async Task<IResult> Handle(
        Guid id,
        UpdateEmployeeRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new UpdateEmployeeCommand(id, request, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Ok(ApiResponse<EmployeeDetailDto>.Ok(result));
    }
}
