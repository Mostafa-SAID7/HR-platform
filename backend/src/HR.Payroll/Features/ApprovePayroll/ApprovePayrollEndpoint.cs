namespace HR.Payroll.Features.ApprovePayroll;

using MediatR;
using HR.Payroll.Application.Dtos.Payroll;
using HR.Common;

/// <summary>
/// Endpoint for approving payroll.
/// </summary>
public static class ApprovePayrollEndpoint
{
    /// <summary>
    /// Handle approve payroll request.
    /// </summary>
    public static async Task<IResult> Handle(
        Guid payrollId,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new ApprovePayrollCommand(payrollId, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Ok(ApiResponse<PayrollRecordDto>.Ok(result));
    }
}
