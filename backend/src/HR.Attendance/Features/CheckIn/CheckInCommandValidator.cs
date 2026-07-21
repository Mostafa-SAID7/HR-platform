namespace HR.Attendance.Features.CheckIn;

using FluentValidation;

/// <summary>
/// Validator for CheckInCommand.
/// SOLID: Validator separated from command and handler.
/// </summary>
public class CheckInCommandValidator : AbstractValidator<CheckInCommand>
{
    public CheckInCommandValidator()
    {
        RuleFor(x => x.Request.EmployeeId).NotEmpty().WithMessage("Employee ID is required");
        RuleFor(x => x.Request.EmployeeName).NotEmpty().WithMessage("Employee name is required");
        RuleFor(x => x.Request.Location).NotEmpty().WithMessage("Location is required");
    }
}
