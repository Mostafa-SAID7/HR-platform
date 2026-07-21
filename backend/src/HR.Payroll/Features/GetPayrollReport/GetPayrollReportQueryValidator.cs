namespace HR.Payroll.Features.GetPayrollReport;

using FluentValidation;

public class GetPayrollReportQueryValidator : AbstractValidator<GetPayrollReportQuery>
{
    public GetPayrollReportQueryValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, DateTime.Now.Year + 1).WithMessage("Year must be valid");
        RuleFor(x => x.Month).InclusiveBetween(1, 12).WithMessage("Month must be between 1 and 12");
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("Tenant ID is required");
    }
}
