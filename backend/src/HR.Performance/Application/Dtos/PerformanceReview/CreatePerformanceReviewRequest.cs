namespace HR.Performance.Application.Dtos.PerformanceReview;

/// <summary>
/// Create performance review request DTO.
/// </summary>
public record CreatePerformanceReviewRequest
{
    public Guid EmployeeId { get; set; }
    public Guid ReviewerId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string ReviewerName { get; set; } = string.Empty;
    public int ReviewYear { get; set; }
    public int ReviewQuarter { get; set; }
}
