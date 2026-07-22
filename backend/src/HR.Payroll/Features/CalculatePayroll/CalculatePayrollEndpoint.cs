namespace HR.Payroll.Features.CalculatePayroll;

using MediatR;
using HR.Payroll.Application.Dtos.Payroll;
using HR.Common;

/// <summary>
/// Endpoint for calculating payroll.
/// </summary>
public static class CalculatePayrollEndpoint
{
    /// <summary>
    /// Handle calculate payroll request.
    /// </summary>
    public static async Task<IResult> Handle(
        CalculatePayrollRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new CalculatePayrollCommand(request, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Ok(ApiResponse<PayrollRecordDto>.Ok(result));
    }
}
