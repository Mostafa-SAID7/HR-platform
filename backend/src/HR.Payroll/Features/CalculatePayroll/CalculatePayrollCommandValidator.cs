namespace HR.Payroll.Features.CalculatePayroll;

using FluentValidation;
using HR.Payroll.Application.Dtos.Payroll;

public class CalculatePayrollCommandValidator : AbstractValidator<CalculatePayrollCommand>
{
    public CalculatePayrollCommandValidator()
    {
        RuleFor(x => x.Request.EmployeeId).NotEmpty().WithMessage("Employee ID is required");
        RuleFor(x => x.Request.BasicSalary).GreaterThan(0).WithMessage("Basic salary must be greater than 0");
        RuleFor(x => x.Request.Year).InclusiveBetween(2000, DateTime.Now.Year + 1);
        RuleFor(x => x.Request.Month).InclusiveBetween(1, 12);
    }
}
