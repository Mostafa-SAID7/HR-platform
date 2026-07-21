namespace HR.Payroll.Features.AddDeduction;

using FluentValidation;
using HR.Payroll.Application.Dtos.Deduction;

public class AddDeductionCommandValidator : AbstractValidator<AddDeductionCommand>
{
    public AddDeductionCommandValidator()
    {
        RuleFor(x => x.Request.PayrollRecordId).NotEmpty().WithMessage("Payroll Record ID is required");
        RuleFor(x => x.Request.EmployeeId).NotEmpty().WithMessage("Employee ID is required");
        RuleFor(x => x.Request.DeductionType).NotEmpty().WithMessage("Deduction Type is required");
        RuleFor(x => x.Request.Amount).GreaterThan(0).WithMessage("Amount must be greater than 0");
    }
}
