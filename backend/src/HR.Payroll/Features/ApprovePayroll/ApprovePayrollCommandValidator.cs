namespace HR.Payroll.Features.ApprovePayroll;

using FluentValidation;

public class ApprovePayrollCommandValidator : AbstractValidator<ApprovePayrollCommand>
{
    public ApprovePayrollCommandValidator()
    {
        RuleFor(x => x.PayrollRecordId).NotEmpty().WithMessage("Payroll Record ID is required");
        RuleFor(x => x.ApprovedBy).NotEmpty().WithMessage("Approved By user ID is required");
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("Tenant ID is required");
    }
}
