namespace HR.Performance.Application.Dtos.PerformanceReview;

using HR.Common.Application.Dtos;

/// <summary>
/// Performance review filter DTO.
/// </summary>
public record PerformanceReviewFilterDto : FilterDto
{
    public Guid? EmployeeId { get; set; }
    public int? ReviewYear { get; set; }
    public string? Status { get; set; }
    public decimal? MinRating { get; set; }
}
