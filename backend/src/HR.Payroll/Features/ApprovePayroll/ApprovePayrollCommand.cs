namespace HR.Payroll.Features.ApprovePayroll;

public record ApprovePayrollCommand(Guid PayrollRecordId, Guid ApprovedBy, Guid TenantId) : ICommand;

public class ApprovePayrollCommandHandler : IRequestHandler<ApprovePayrollCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ApprovePayrollCommandHandler> _logger;

    public ApprovePayrollCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<ApprovePayrollCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(ApprovePayrollCommand request, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.GetRepository<PayrollRecord>();

        var payroll = await repo.GetAsQueryable()
            .FirstOrDefaultAsync(p => p.Id == request.PayrollRecordId && 
                p.TenantId == request.TenantId, cancellationToken);

        if (payroll is null)
            throw new NotFoundException("PayrollRecord", request.PayrollRecordId);

        if (payroll.Status != "Processed")
            throw new DomainException($"Payroll must be in 'Processed' status to approve. Current status: {payroll.Status}");

        payroll.Approve(request.ApprovedBy);
        repo.Update(payroll);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Payroll {PayrollRecordId} approved by {ApprovedBy}",
            request.PayrollRecordId, request.ApprovedBy);
    }
}
