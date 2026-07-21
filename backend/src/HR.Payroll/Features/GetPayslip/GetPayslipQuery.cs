namespace HR.Payroll.Features.GetPayslip;

using Dapper;

public record GetPayslipQuery(Guid PayrollRecordId, Guid TenantId) : IQuery<PayslipDto>;

public class GetPayslipQueryHandler : IRequestHandler<GetPayslipQuery, PayslipDto>
{
    private readonly IQueryRepository _queryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetPayslipQueryHandler> _logger;

    public GetPayslipQueryHandler(
        IQueryRepository queryRepository,
        IUnitOfWork unitOfWork,
        ILogger<GetPayslipQueryHandler> logger)
    {
        _queryRepository = queryRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PayslipDto> Handle(GetPayslipQuery request, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT 
                Id as PayrollRecordId,
                EmployeeId,
                EmployeeName,
                Year,
                Month,
                BasicSalary,
                GrossIncome,
                IncomeTax,
                ""SocialSecurityContribution"",
                HealthInsurance,
                NetSalary
            FROM PayrollRecords
            WHERE Id = @PayrollRecordId 
                AND TenantId = @TenantId
                AND IsDeleted = false";

        var parameters = new DynamicParameters();
        parameters.Add("@PayrollRecordId", request.PayrollRecordId);
        parameters.Add("@TenantId", request.TenantId);

        var result = await _queryRepository.QuerySingleOrDefaultAsync<PayslipDto>(sql, parameters);

        if (result is null)
        {
            throw new NotFoundException("PayrollRecord", request.PayrollRecordId);
        }

        _logger.LogInformation(
            "Payslip retrieved for payroll record {PayrollRecordId}",
            request.PayrollRecordId);

        return result;
    }
}
