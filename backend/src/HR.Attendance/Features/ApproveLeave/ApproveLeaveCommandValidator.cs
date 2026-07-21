namespace HR.Attendance.Features.ApproveLeave;

using FluentValidation;

public class ApproveLeaveCommandValidator : AbstractValidator<ApproveLeaveCommand>
{
    public ApproveLeaveCommandValidator()
    {
        RuleFor(x => x.LeaveRequestId)
            .NotEmpty()
            .WithMessage("LeaveRequestId is required");

        RuleFor(x => x.ApprovedBy)
            .NotEmpty()
            .WithMessage("ApprovedBy is required");

        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("TenantId is required");
    }
}
