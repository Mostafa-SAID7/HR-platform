namespace HR.Attendance.Features.CheckOut;

using FluentValidation;

public class CheckOutCommandValidator : AbstractValidator<CheckOutCommand>
{
    public CheckOutCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("CheckOut request cannot be null");

        RuleFor(x => x.Request.EmployeeId)
            .NotEmpty()
            .WithMessage("EmployeeId is required");

        RuleFor(x => x.Request.CheckOutTime)
            .NotEmpty()
            .WithMessage("CheckOutTime is required");

        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("TenantId is required");
    }
}
