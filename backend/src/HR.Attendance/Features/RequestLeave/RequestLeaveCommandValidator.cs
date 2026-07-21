namespace HR.Attendance.Features.RequestLeave;

using FluentValidation;

public class RequestLeaveCommandValidator : AbstractValidator<RequestLeaveCommand>
{
    public RequestLeaveCommandValidator()
    {
        RuleFor(x => x.Request.EmployeeId)
            .NotEmpty()
            .WithMessage("Employee ID is required");

        RuleFor(x => x.Request.LeaveType)
            .NotEmpty()
            .WithMessage("Leave type is required");

        RuleFor(x => x.Request.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required");

        RuleFor(x => x.Request.EndDate)
            .NotEmpty()
            .WithMessage("End date is required")
            .GreaterThanOrEqualTo(x => x.Request.StartDate)
            .WithMessage("End date must be greater than or equal to start date");

        RuleFor(x => x.Request.Reason)
            .NotEmpty()
            .WithMessage("Reason is required");

        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("TenantId is required");
    }
}
