namespace HR.Payroll.Features.AddDeduction;

public record AddDeductionCommand(AddDeductionRequest Request, Guid TenantId) : ICommand;

public class AddDeductionCommandValidator : AbstractValidator<AddDeductionCommand>
{
    public AddDeductionCommandValidator()
    {
        RuleFor(x => x.Request.PayrollRecordId).NotEmpty();
        RuleFor(x => x.Request.EmployeeId).NotEmpty();
        RuleFor(x => x.Request.DeductionType).NotEmpty();
        RuleFor(x => x.Request.Amount).GreaterThan(0);
    }
}

public class AddDeductionCommandHandler : IRequestHandler<AddDeductionCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddDeductionCommandHandler> _logger;

    public AddDeductionCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<AddDeductionCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(AddDeductionCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        var deduction = Deduction.Create(
            req.PayrollRecordId,
            req.EmployeeId,
            req.DeductionType,
            req.DeductionName,
            req.Amount,
            request.TenantId);

        deduction.Description = req.Description;

        var repo = _unitOfWork.GetRepository<Deduction>();
        await repo.AddAsync(deduction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Deduction added: {DeductionType} - {Amount} for employee {EmployeeId}",
            req.DeductionType, req.Amount, req.EmployeeId);
    }
}
