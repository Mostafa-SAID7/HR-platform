namespace HR.Payroll.Features.GetPayrollReport;

using MediatR;
using HR.Payroll.Application.Dtos.Report;
using HR.Common;

/// <summary>
/// Endpoint for retrieving payroll reports.
/// </summary>
public static class GetPayrollReportEndpoint
{
    /// <summary>
    /// Handle get payroll report request.
    /// </summary>
    public static async Task<IResult> Handle(
        int year,
        int month,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var query = new GetPayrollReportQuery(year, month, tenantId);
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(ApiResponse<PayrollReportDto>.Ok(result));
    }
}
