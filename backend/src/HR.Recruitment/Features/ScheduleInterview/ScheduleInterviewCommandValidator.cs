namespace HR.Recruitment.Features.ScheduleInterview;

/// <summary>
/// Validator for ScheduleInterviewCommand
/// </summary>
public class ScheduleInterviewCommandValidator : AbstractValidator<ScheduleInterviewCommand>
{
    public ScheduleInterviewCommandValidator()
    {
        RuleFor(x => x.JobApplicationId)
            .NotEmpty().WithMessage("Job application ID is required");

        RuleFor(x => x.Request.ScheduledDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Interview must be scheduled in the future");

        RuleFor(x => x.Request.ScheduledEndTime)
            .GreaterThan(x => x.Request.ScheduledDate)
            .WithMessage("End time must be after start time");

        RuleFor(x => x.Request.InterviewType)
            .NotEmpty().WithMessage("Interview type is required")
            .Must(x => new[] { "Phone", "Video", "In-Person" }.Contains(x))
            .WithMessage("Invalid interview type");

        RuleFor(x => x.Request.InterviewerName)
            .NotEmpty().WithMessage("Interviewer name is required");

        RuleFor(x => x.Request.InterviewerEmail)
            .NotEmpty().WithMessage("Interviewer email is required")
            .EmailAddress().WithMessage("Valid email is required");
    }
}
