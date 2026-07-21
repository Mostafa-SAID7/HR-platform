namespace HR.Audit.Features.CreateAuditReport;

using FluentValidation;

/// <summary>
/// Validator for CreateAuditReportCommand
/// </summary>
public class CreateAuditReportCommandValidator : AbstractValidator<CreateAuditReportCommand>
{
    public CreateAuditReportCommandValidator()
    {
        RuleFor(x => x.Request.Title)
            .NotEmpty()
            .WithMessage("Report title is required");

        RuleFor(x => x.Request.ReportType)
            .Must(x => Enum.TryParse<ReportType>(x, true, out _))
            .WithMessage("Invalid report type");

        RuleFor(x => x.Request.EndDate)
            .GreaterThan(x => x.Request.StartDate)
            .When(x => x.Request.StartDate.HasValue && x.Request.EndDate.HasValue)
            .WithMessage("End date must be after start date");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("TenantId is required");
    }
}
