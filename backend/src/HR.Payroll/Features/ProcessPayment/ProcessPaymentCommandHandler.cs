namespace HR.Payroll.Features.ProcessPayment;

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HR.Payroll.Domain;
using HR.Common.UnitOfWork;
using HR.Common.Exceptions;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessPaymentCommandHandler> _logger;

    public ProcessPaymentCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<ProcessPaymentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.GetRepository<PayrollRecord>();

        var payroll = await repo.GetAsQueryable()
            .FirstOrDefaultAsync(p => p.Id == request.PayrollRecordId && 
                p.TenantId == request.TenantId, cancellationToken);

        if (payroll is null)
            throw new NotFoundException(nameof(PayrollRecord), request.PayrollRecordId);

        if (payroll.Status != "Approved")
            throw new DomainException($"Payroll must be in 'Approved' status to process payment. Current status: {payroll.Status}");

        payroll.MarkAsPaid();
        repo.Update(payroll);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Payment processed for payroll {PayrollRecordId}. Amount: {Amount}",
            request.PayrollRecordId, payroll.NetSalary);
    }
}
