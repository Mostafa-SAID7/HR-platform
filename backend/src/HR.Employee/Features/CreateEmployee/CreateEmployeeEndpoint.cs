namespace HR.Employee.Features.CreateEmployee;

using MediatR;
using HR.Common.Domain;
using HR.Employee.Application.Dtos.Employee;

/// <summary>
/// Endpoint for creating a new employee
/// </summary>
public static class CreateEmployeeEndpoint
{
    public static async Task<IResult> Handle(
        CreateEmployeeRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new CreateEmployeeCommand(request, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Created($"/employees/{result.Id}", ApiResponse<EmployeeDetailDto>.Created(result));
    }
}
