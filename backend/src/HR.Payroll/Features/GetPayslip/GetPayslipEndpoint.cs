namespace HR.Payroll.Features.GetPayslip;

using MediatR;
using HR.Payroll.Application.Dtos.Payslip;
using HR.Common;

/// <summary>
/// Endpoint for retrieving payslips.
/// </summary>
public static class GetPayslipEndpoint
{
    /// <summary>
    /// Handle get payslip request.
    /// </summary>
    public static async Task<IResult> Handle(
        Guid payslipId,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var query = new GetPayslipQuery(payslipId, tenantId);
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(ApiResponse<PayslipDto>.Ok(result));
    }
}
