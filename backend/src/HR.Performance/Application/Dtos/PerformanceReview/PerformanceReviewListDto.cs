namespace HR.Performance.Application.Dtos.PerformanceReview;

/// <summary>
/// Performance review list DTO.
/// </summary>
public record PerformanceReviewListDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int ReviewYear { get; set; }
    public int ReviewQuarter { get; set; }
    public decimal PerformanceRating { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ReviewDate { get; set; }
    public DateTime? CompletedDate { get; set; }
}
