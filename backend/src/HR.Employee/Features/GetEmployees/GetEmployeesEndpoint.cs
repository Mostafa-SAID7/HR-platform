namespace HR.Employee.Features.GetEmployees;

using MediatR;
using HR.Common.Domain;
using HR.Employee.Application.Dtos.Employee;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Endpoint for retrieving employees with pagination and filtering
/// </summary>
public static class GetEmployeesEndpoint
{
    public static async Task<IResult> Handle(
        [AsParameters] EmployeeFilterDto filter,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var query = new GetEmployeesQuery(filter, tenantId);
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(ApiResponse<PaginatedResult<EmployeeListDto>>.Ok(result));
    }
}
