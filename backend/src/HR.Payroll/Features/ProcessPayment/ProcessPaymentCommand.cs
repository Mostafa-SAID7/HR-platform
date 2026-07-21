namespace HR.Payroll.Features.ProcessPayment;

/// <summary>
/// Process payment for an approved payroll
/// </summary>
public record ProcessPaymentCommand(
    Guid PayrollRecordId,
    Guid TenantId) : ICommand;
