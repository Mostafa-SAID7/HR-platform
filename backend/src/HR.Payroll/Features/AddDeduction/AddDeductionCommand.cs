namespace HR.Payroll.Features.AddDeduction;

using HR.Payroll.Application.Dtos.Deduction;

/// <summary>
/// Add a deduction to a payroll record
/// </summary>
public record AddDeductionCommand(
    AddDeductionRequest Request,
    Guid TenantId) : ICommand;
