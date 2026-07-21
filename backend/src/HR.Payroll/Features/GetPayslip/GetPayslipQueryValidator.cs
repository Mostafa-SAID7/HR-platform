namespace HR.Payroll.Features.GetPayslip;

using FluentValidation;

public class GetPayslipQueryValidator : AbstractValidator<GetPayslipQuery>
{
    public GetPayslipQueryValidator()
    {
        RuleFor(x => x.PayrollRecordId).NotEmpty().WithMessage("Payroll Record ID is required");
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("Tenant ID is required");
    }
}
