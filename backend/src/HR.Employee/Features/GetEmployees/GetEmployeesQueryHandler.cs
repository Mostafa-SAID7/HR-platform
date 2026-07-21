namespace HR.Employee.Features.GetEmployees;

using MediatR;
using HR.Employee.Domain.Employee;
using HR.Employee.Application.Dtos.Employee;
using HR.Common.Application.Abstractions;
using HR.Common.Application.Helpers;

/// <summary>
/// Handler for GetEmployeesQuery using Dapper for performance.
/// </summary>
public class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, PaginatedResult<EmployeeDto>>
{
    private readonly IQueryRepository _queryRepository;
    private readonly ILogger<GetEmployeesQueryHandler> _logger;

    public GetEmployeesQueryHandler(
        IQueryRepository queryRepository,
        ILogger<GetEmployeesQueryHandler> logger)
    {
        _queryRepository = queryRepository;
        _logger = logger;
    }

    public async Task<PaginatedResult<EmployeeDto>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        filter.Validate();

        var skip = PaginationHelper.CalculateSkip(filter.PageNumber, filter.PageSize);

        // Build SQL query with filters
        var whereClauses = new List<string> { "e.is_deleted = false AND e.tenant_id = @tenantId" };
        var parameters = new { tenantId = request.TenantId };

        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            whereClauses.Add("(e.first_name ILIKE @searchTerm OR e.last_name ILIKE @searchTerm OR e.email ILIKE @searchTerm)");
            parameters = new { tenantId = request.TenantId, searchTerm = $"%{filter.SearchTerm}%" };
        }

        if (!string.IsNullOrEmpty(filter.Status))
        {
            whereClauses.Add("e.status = @status");
        }

        if (!string.IsNullOrEmpty(filter.EmploymentType))
        {
            whereClauses.Add("e.employment_type = @employmentType");
        }

        if (filter.IsActive.HasValue)
        {
            whereClauses.Add("e.is_active = @isActive");
        }

        var whereClause = string.Join(" AND ", whereClauses);

        // Get total count
        var countSql = $@"
            SELECT COUNT(*) 
            FROM employees e
            WHERE {whereClause}";

        var totalCount = await _queryRepository.QuerySingleOrDefaultAsync<int>(countSql, parameters, cancellationToken) ?? 0;

        // Get paginated results
        var sql = $@"
            SELECT 
                e.id,
                e.first_name,
                e.last_name,
                e.email,
                e.job_title,
                d.name as department_name,
                e.status,
                e.hire_date
            FROM employees e
            LEFT JOIN departments d ON e.department_id = d.id
            WHERE {whereClause}
            ORDER BY e.created_on_utc DESC
            LIMIT @pageSize OFFSET @skip";

        var items = (await _queryRepository.QueryAsync<EmployeeDto>(sql, 
            new { tenantId = request.TenantId, skip, pageSize = filter.PageSize }, 
            cancellationToken)).ToList();

        _logger.LogInformation("Retrieved {Count} employees for tenant {TenantId}", items.Count, request.TenantId);

        return PaginatedResult<EmployeeDto>.Create(items, filter.PageNumber, filter.PageSize, totalCount);
    }
}
