namespace HR.Analytics.Features.Search;

/// <summary>
/// Query definition for searching employees.
/// SOLID: Query record separated from handler.
/// </summary>
public record SearchEmployeesQuery(string SearchTerm, int PageSize, int PageNumber, Guid TenantId) : IQuery<SearchResultDto<EmployeeAnalyticsDto>>;
