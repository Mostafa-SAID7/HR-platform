namespace HR.Performance.Application.Dtos.PerformanceReview;

/// <summary>
/// Approve performance review request DTO.
/// </summary>
public record ApprovePerformanceReviewRequest
{
    public string ApprovalComments { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
}
