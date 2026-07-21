namespace HR.Employee.Features.TerminateEmployee;

using FluentValidation;

/// <summary>
/// Validator for TerminateEmployeeCommand.
/// </summary>
public class TerminateEmployeeCommandValidator : AbstractValidator<TerminateEmployeeCommand>
{
    public TerminateEmployeeCommandValidator()
    {
        RuleFor(x => x.TerminationDate)
            .NotEmpty().WithMessage("Termination date is required")
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Termination date cannot be in the past");
    }
}
