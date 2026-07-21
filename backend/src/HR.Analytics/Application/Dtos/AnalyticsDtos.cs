namespace HR.Analytics.Application.Dtos;

/// <summary>
/// Search query parameters for full-text search.
/// </summary>
public record SearchQuery
{
    public string SearchTerm { get; set; } = string.Empty;
    public string IndexName { get; set; } = string.Empty; // employees, payroll, etc.
    public int PageSize { get; set; } = 20;
    public int PageNumber { get; set; } = 1;
    public Dictionary<string, string>? Filters { get; set; }
}

/// <summary>
/// Search result wrapper.
/// </summary>
public record SearchResultDto<T>
{
    public List<T> Results { get; set; } = new();
    public long TotalCount { get; set; }
    public int PageSize { get; set; }
    public int PageNumber { get; set; }
}

/// <summary>
/// Employee analytics DTO.
/// </summary>
public record EmployeeAnalyticsDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public DateTime JoinDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int YearsOfService { get; set; }
    public int SkillCount { get; set; }
}

/// <summary>
/// Payroll analytics DTO.
/// </summary>
public record PayrollAnalyticsDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal GrossIncome { get; set; }
    public decimal NetSalary { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Performance analytics DTO.
/// </summary>
public record PerformanceAnalyticsDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Quarter { get; set; }
    public decimal AverageRating { get; set; }
    public int GoalsCompleted { get; set; }
}

/// <summary>
/// Dashboard metrics DTO.
/// </summary>
public record DashboardMetricsDto
{
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public decimal AverageBasicSalary { get; set; }
    public decimal AveragePerformanceRating { get; set; }
    public decimal AverageAttendancePercentage { get; set; }
    public int TotalDepartments { get; set; }
    public DateTime ComputedDate { get; set; }
}

/// <summary>
/// Report generation request.
/// </summary>
public record GenerateReportRequest
{
    public string ReportType { get; set; } = string.Empty; // EmployeeReport, PayrollReport, etc.
    public int Year { get; set; }
    public int? Month { get; set; }
    public string? Department { get; set; }
}

/// <summary>
/// Report DTO.
/// </summary>
public record ReportDto
{
    public Guid ReportId { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public DateTime GeneratedDate { get; set; }
    public int RecordCount { get; set; }
    public string ReportUrl { get; set; } = string.Empty; // URL to download report
}
