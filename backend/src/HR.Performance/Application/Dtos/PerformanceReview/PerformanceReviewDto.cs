namespace HR.Performance.Application.Dtos.PerformanceReview;

/// <summary>
/// Performance review basic DTO.
/// </summary>
public record PerformanceReviewDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid ReviewerId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string ReviewerName { get; set; } = string.Empty;
    public int ReviewYear { get; set; }
    public int ReviewQuarter { get; set; }
    public DateTime ReviewDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsFinal { get; set; }
}
