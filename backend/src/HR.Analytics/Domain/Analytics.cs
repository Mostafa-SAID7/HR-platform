namespace HR.Analytics.Domain;

/// <summary>
/// Analytics event for data warehouse ingestion.
/// </summary>
public class AnalyticsEvent : AggregateRoot
{
    public string EventType { get; set; } = string.Empty; // EmployeeCreated, PayrollProcessed, etc.
    public string EntityType { get; set; } = string.Empty; // Employee, Payroll, etc.
    public Guid EntityId { get; set; }
    public Dictionary<string, object> EventData { get; set; } = new();
    public DateTime EventTimestamp { get; set; }
    public bool SyncedToElasticsearch { get; set; }
    public DateTime? ElasticsearchSyncTime { get; set; }
}

/// <summary>
/// Employee analytics view (denormalized from Employee Service).
/// </summary>
public class EmployeeAnalytics : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public DateTime JoinDate { get; set; }
    public string Status { get; set; } = string.Empty; // Active, Inactive, Terminated
    public int YearsOfService { get; set; }
    public int SkillCount { get; set; }
    public decimal? CurrentSalary { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Payroll analytics view (denormalized from Payroll Service).
/// </summary>
public class PayrollAnalytics : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal GrossIncome { get; set; }
    public decimal IncomeTax { get; set; }
    public decimal NetSalary { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ProcessedDate { get; set; }
}

/// <summary>
/// Performance analytics view (denormalized from Performance Service).
/// </summary>
public class PerformanceAnalytics : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Quarter { get; set; }
    public decimal AverageRating { get; set; }
    public int GoalsSet { get; set; }
    public int GoalsCompleted { get; set; }
    public int FeedbackCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ReviewDate { get; set; }
}

/// <summary>
/// Attendance analytics view (denormalized from Attendance Service).
/// </summary>
public class AttendanceAnalytics : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int LateDays { get; set; }
    public int LeaveDays { get; set; }
    public decimal AverageWorkHours { get; set; }
    public DateTime ReportDate { get; set; }
}

/// <summary>
/// Dashboard metrics for quick overview.
/// </summary>
public class DashboardMetrics : BaseEntity
{
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int InactiveEmployees { get; set; }
    public decimal AverageBasicSalary { get; set; }
    public decimal AverageNetSalary { get; set; }
    public decimal AveragePerformanceRating { get; set; }
    public decimal AverageAttendancePercentage { get; set; }
    public int TotalDepartments { get; set; }
    public DateTime ComputedDate { get; set; }
}
