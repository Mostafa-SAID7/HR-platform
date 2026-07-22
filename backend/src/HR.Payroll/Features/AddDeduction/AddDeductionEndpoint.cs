namespace HR.Payroll.Features.AddDeduction;

using MediatR;
using HR.Payroll.Application.Dtos.Deduction;
using HR.Common;

/// <summary>
/// Endpoint for adding a deduction to payroll.
/// </summary>
public static class AddDeductionEndpoint
{
    /// <summary>
    /// Handle add deduction request.
    /// </summary>
    public static async Task<IResult> Handle(
        AddDeductionRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new AddDeductionCommand(request, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Created($"/payroll/deductions/{result.Id}", ApiResponse<DeductionDto>.Created(result));
    }
}
