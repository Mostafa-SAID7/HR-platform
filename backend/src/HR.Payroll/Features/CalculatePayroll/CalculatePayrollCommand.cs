namespace HR.Payroll.Features.CalculatePayroll;

public record CalculatePayrollCommand(CalculatePayrollRequest Request, Guid TenantId) : ICommand<PayrollRecordDto>;

public class CalculatePayrollCommandValidator : AbstractValidator<CalculatePayrollCommand>
{
    public CalculatePayrollCommandValidator()
    {
        RuleFor(x => x.Request.EmployeeId).NotEmpty().WithMessage("Employee ID is required");
        RuleFor(x => x.Request.BasicSalary).GreaterThan(0).WithMessage("Basic salary must be greater than 0");
        RuleFor(x => x.Request.Year).InclusiveBetween(2000, DateTime.Now.Year + 1);
        RuleFor(x => x.Request.Month).InclusiveBetween(1, 12);
    }
}

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
            req.EmployeeName,
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
