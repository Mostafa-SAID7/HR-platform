namespace HR.Employee.Features.UpdateEmployee;

using FluentValidation;
using HR.Employee.Application.Dtos.Employee;

/// <summary>
/// Validator for UpdateEmployeeCommand.
/// </summary>
public class UpdateEmployeeCommandValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeCommandValidator()
    {
        RuleFor(x => x.Request.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.Request.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.Request.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required");

        RuleFor(x => x.Request.JobTitle)
            .NotEmpty().WithMessage("Job title is required");

        RuleFor(x => x.Request.DepartmentId)
            .NotEmpty().WithMessage("Department is required");

        RuleFor(x => x.Request.Salary)
            .GreaterThan(0).WithMessage("Salary must be greater than 0");
    }
}
