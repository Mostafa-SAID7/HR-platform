namespace HR.Payroll.Features.GetPayslip;

using MediatR;
using Dapper;
using Microsoft.Extensions.Logging;
using HR.Payroll.Application.Dtos.Payslip;
using HR.Common.QueryRepository;
using HR.Common.Exceptions;

public class GetPayslipQueryHandler : IRequestHandler<GetPayslipQuery, PayslipDto>
{
    private readonly IQueryRepository _queryRepository;
    private readonly ILogger<GetPayslipQueryHandler> _logger;

    public GetPayslipQueryHandler(
        IQueryRepository queryRepository,
        ILogger<GetPayslipQueryHandler> logger)
    {
        _queryRepository = queryRepository;
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
            throw new NotFoundException(nameof(PayrollRecord), request.PayrollRecordId);
        }

        _logger.LogInformation(
            "Payslip retrieved for payroll record {PayrollRecordId}",
            request.PayrollRecordId);

        return result;
    }
}
