namespace HR.Performance.Application.Dtos.PerformanceReview;

/// <summary>
/// Set performance ratings request DTO.
/// </summary>
public record SetPerformanceRatingsRequest
{
    public decimal PerformanceRating { get; set; }
    public decimal ProductivityRating { get; set; }
    public decimal QualityRating { get; set; }
    public decimal TeamworkRating { get; set; }
    public decimal LeadershipRating { get; set; }
    public string Comments { get; set; } = string.Empty;
    public string StrengthAreas { get; set; } = string.Empty;
    public string ImprovementAreas { get; set; } = string.Empty;
}
