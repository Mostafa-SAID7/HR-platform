namespace HR.Payroll.Features.GetPayrollReport;

using Dapper;

public record GetPayrollReportQuery(int Year, int Month, Guid TenantId) : IQuery<PayrollReportDto>;

public class GetPayrollReportQueryHandler : IRequestHandler<GetPayrollReportQuery, PayrollReportDto>
{
    private readonly IQueryRepository _queryRepository;
    private readonly ILogger<GetPayrollReportQueryHandler> _logger;

    public GetPayrollReportQueryHandler(
        IQueryRepository queryRepository,
        ILogger<GetPayrollReportQueryHandler> logger)
    {
        _queryRepository = queryRepository;
        _logger = logger;
    }

    public async Task<PayrollReportDto> Handle(GetPayrollReportQuery request, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT 
                COUNT(DISTINCT EmployeeId) as TotalEmployees,
                SUM(CASE WHEN Status IN ('Processed', 'Approved', 'Paid') THEN 1 ELSE 0 END) as ProcessedCount,
                SUM(CASE WHEN Status IN ('Approved', 'Paid') THEN 1 ELSE 0 END) as ApprovedCount,
                SUM(CASE WHEN Status = 'Paid' THEN 1 ELSE 0 END) as PaidCount,
                COALESCE(SUM(GrossIncome), 0) as TotalGrossIncome,
                COALESCE(SUM(IncomeTax), 0) as TotalIncomeTax,
                COALESCE(SUM(IncomeTax + ""SocialSecurityContribution"" + HealthInsurance), 0) as TotalDeductions,
                COALESCE(SUM(NetSalary), 0) as TotalNetSalary
            FROM PayrollRecords
            WHERE Year = @Year 
                AND Month = @Month 
                AND TenantId = @TenantId
                AND IsDeleted = false";

        var parameters = new DynamicParameters();
        parameters.Add("@Year", request.Year);
        parameters.Add("@Month", request.Month);
        parameters.Add("@TenantId", request.TenantId);

        var result = await _queryRepository.QuerySingleOrDefaultAsync<dynamic>(sql, parameters);

        if (result is null)
        {
            _logger.LogWarning("No payroll records found for {Year}/{Month}", request.Year, request.Month);
            return new PayrollReportDto
            {
                ReportGeneratedDate = DateTime.UtcNow,
                TotalEmployees = 0,
                ProcessedCount = 0,
                ApprovedCount = 0,
                PaidCount = 0
            };
        }

        var report = new PayrollReportDto
        {
            TotalEmployees = result.TotalEmployees ?? 0,
            ProcessedCount = result.ProcessedCount ?? 0,
            ApprovedCount = result.ApprovedCount ?? 0,
            PaidCount = result.PaidCount ?? 0,
            TotalGrossIncome = result.TotalGrossIncome ?? 0,
            TotalIncomeTax = result.TotalIncomeTax ?? 0,
            TotalDeductions = result.TotalDeductions ?? 0,
            TotalNetSalary = result.TotalNetSalary ?? 0,
            ReportGeneratedDate = DateTime.UtcNow
        };

        _logger.LogInformation(
            "Payroll report generated for {Year}/{Month}. Total employees: {Employees}, Total net salary: {TotalSalary}",
            request.Year, request.Month, report.TotalEmployees, report.TotalNetSalary);

        return report;
    }
}
