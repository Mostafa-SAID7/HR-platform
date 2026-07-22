namespace HR.Payroll.Features.ProcessPayment;

using MediatR;
using HR.Payroll.Application.Dtos.Payroll;
using HR.Common;

/// <summary>
/// Endpoint for processing payroll payments.
/// </summary>
public static class ProcessPaymentEndpoint
{
    /// <summary>
    /// Handle process payment request.
    /// </summary>
    public static async Task<IResult> Handle(
        ProcessPaymentRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new ProcessPaymentCommand(request, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Ok(ApiResponse<PayrollRecordDto>.Ok(result));
    }
}
