namespace HR.Employee.Features.CreateEmployee;

using FluentValidation;
using HR.Employee.Application.Dtos.Employee;

/// <summary>
/// Validator for CreateEmployeeCommand.
/// </summary>
public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(x => x.Request.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.Request.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.Request.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Request.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^\d{10,}$").WithMessage("Phone number must have at least 10 digits");

        RuleFor(x => x.Request.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required")
            .LessThan(DateTime.Today.AddYears(-18)).WithMessage("Employee must be at least 18 years old");

        RuleFor(x => x.Request.Gender)
            .NotEmpty().WithMessage("Gender is required")
            .Must(g => g is "Male" or "Female" or "Other").WithMessage("Invalid gender value");

        RuleFor(x => x.Request.NationalId)
            .NotEmpty().WithMessage("National ID is required")
            .MaximumLength(50).WithMessage("National ID must not exceed 50 characters");

        RuleFor(x => x.Request.HireDate)
            .NotEmpty().WithMessage("Hire date is required")
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Hire date cannot be in the future");

        RuleFor(x => x.Request.DepartmentId)
            .NotEmpty().WithMessage("Department is required");

        RuleFor(x => x.Request.JobTitle)
            .NotEmpty().WithMessage("Job title is required")
            .MaximumLength(256).WithMessage("Job title must not exceed 256 characters");

        RuleFor(x => x.Request.EmploymentType)
            .NotEmpty().WithMessage("Employment type is required")
            .Must(et => et is "Full-time" or "Part-time" or "Contract")
            .WithMessage("Invalid employment type");

        RuleFor(x => x.Request.Salary)
            .GreaterThan(0).WithMessage("Salary must be greater than 0");
    }
}
