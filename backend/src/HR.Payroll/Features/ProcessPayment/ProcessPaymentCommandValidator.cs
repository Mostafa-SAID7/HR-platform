namespace HR.Payroll.Features.ProcessPayment;

using FluentValidation;

public class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
{
    public ProcessPaymentCommandValidator()
    {
        RuleFor(x => x.PayrollRecordId).NotEmpty().WithMessage("Payroll Record ID is required");
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("Tenant ID is required");
    }
}
