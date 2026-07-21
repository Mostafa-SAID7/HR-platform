namespace HR.Payroll.Features.CalculatePayroll;

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HR.Payroll.Domain;
using HR.Payroll.Application.Dtos.Payroll;
using HR.Common.UnitOfWork;
using HR.Common.Exceptions;

public class CalculatePayrollCommandHandler : IRequestHandler<CalculatePayrollCommand, PayrollRecordDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CalculatePayrollCommandHandler> _logger;
    private const decimal DEFAULT_INCOME_TAX_RATE = 10m;
    private const decimal DEFAULT_SSC_RATE = 5m;
    private const decimal DEFAULT_HEALTH_INSURANCE = 500m;

    public CalculatePayrollCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CalculatePayrollCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PayrollRecordDto> Handle(CalculatePayrollCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        var repo = _unitOfWork.GetRepository<PayrollRecord>();

        // Check if payroll already exists for this month
        var existingPayroll = await repo.GetAsQueryable()
            .FirstOrDefaultAsync(p => p.EmployeeId == req.EmployeeId && 
                p.Year == req.Year && 
                p.Month == req.Month && 
                p.TenantId == request.TenantId, cancellationToken);

        if (existingPayroll is not null && existingPayroll.Status != "Draft")
        {
            throw new DomainException($"Payroll already processed for {req.Month}/{req.Year}");
        }

        var payroll = existingPayroll ?? PayrollRecord.Create(
            req.EmployeeId,
            req.EmployeeName ?? string.Empty,
            req.BasicSalary,
            req.Year,
            req.Month,
            request.TenantId);

        payroll.HousingAllowance = req.HousingAllowance;
        payroll.TransportAllowance = req.TransportAllowance;
        payroll.OtherAllowances = req.OtherAllowances;

        // Calculate payroll with default tax rates
        payroll.CalculatePayroll(DEFAULT_INCOME_TAX_RATE, DEFAULT_SSC_RATE, DEFAULT_HEALTH_INSURANCE);

        if (existingPayroll is null)
        {
            await repo.AddAsync(payroll, cancellationToken);
        }
        else
        {
            repo.Update(payroll);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Payroll calculated for employee {EmployeeId} ({Month}/{Year}). Net Salary: {NetSalary}",
            req.EmployeeId, req.Month, req.Year, payroll.NetSalary);

        return MapToDto(payroll);
    }

    private static PayrollRecordDto MapToDto(PayrollRecord record)
    {
        return new PayrollRecordDto
        {
            Id = record.Id,
            EmployeeId = record.EmployeeId,
            EmployeeName = record.EmployeeName,
            Year = record.Year,
            Month = record.Month,
            BasicSalary = record.BasicSalary,
            HousingAllowance = record.HousingAllowance,
            TransportAllowance = record.TransportAllowance,
            OtherAllowances = record.OtherAllowances,
            GrossIncome = record.GrossIncome,
            IncomeTax = record.IncomeTax,
            SocialSecurityContribution = record.SocialSecurityContribution,
            HealthInsurance = record.HealthInsurance,
            NetSalary = record.NetSalary,
            Status = record.Status,
            ProcessedDate = record.ProcessedDate,
            PaidDate = record.PaidDate
        };
    }
}
