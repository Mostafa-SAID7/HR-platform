namespace HR.Employee.Features.GetEmployeeById;

using MediatR;
using HR.Common.Domain;
using HR.Employee.Application.Dtos.Employee;

/// <summary>
/// Endpoint for retrieving an employee by ID
/// </summary>
public static class GetEmployeeByIdEndpoint
{
    public static async Task<IResult> Handle(
        Guid id,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var query = new GetEmployeeByIdQuery(id, tenantId);
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(ApiResponse<EmployeeDetailDto>.Ok(result));
    }
}
