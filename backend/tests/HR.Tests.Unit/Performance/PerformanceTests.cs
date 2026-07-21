namespace HR.Tests.Unit.Performance;

/// <summary>
/// Comprehensive unit tests for Performance Service (14 tests).
/// Tests cover: review creation, feedback, approval, rating, and queries.
/// </summary>
public class PerformanceTests
{
    private readonly Guid _employeeId = Guid.NewGuid();
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public void CreatePerformanceReview_WithValidData_CreatesReview()
    {
        // Arrange
        var reviewPeriod = "Q3 2026";
        var reviewDate = DateTime.Now;

        // Act
        var review = PerformanceReview.Create(_employeeId, reviewPeriod, reviewDate, _tenantId);

        // Assert
        Assert.NotNull(review);
        Assert.NotEqual(Guid.Empty, review.Id);
        Assert.Equal(_employeeId, review.EmployeeId);
        Assert.Equal(reviewPeriod, review.ReviewPeriod);
        Assert.Equal(reviewDate, review.ReviewDate);
        Assert.Equal("Draft", review.Status);
        Assert.Equal(_tenantId, review.TenantId);
    }

    [Fact]
    public void AddFeedback_WithValidData_AddsFeedback()
    {
        // Arrange
        var review = PerformanceReview.Create(_employeeId, "Q3 2026", DateTime.Now, _tenantId);

        // Act
        review.AddFeedback("Communication", "Excellent communication skills");

        // Assert
        Assert.Single(review.Feedback);
        var feedback = review.Feedback.First();
        Assert.Equal("Communication", feedback.Category);
        Assert.Equal("Excellent communication skills", feedback.Comment);
    }

    [Fact]
    public void AddFeedback_MultipleCategories_AddsAll()
    {
        // Arrange
        var review = PerformanceReview.Create(_employeeId, "Q3 2026", DateTime.Now, _tenantId);

        // Act
        review.AddFeedback("Communication", "Excellent");
        review.AddFeedback("Technical Skills", "Very Strong");
        review.AddFeedback("Leadership", "Developing");

        // Assert
        Assert.Equal(3, review.Feedback.Count);
    }

    [Fact]
    public void SetRating_WithValidRating_SetsRating()
    {
        // Arrange
        var review = PerformanceReview.Create(_employeeId, "Q3 2026", DateTime.Now, _tenantId);

        // Act
        review.SetRating("5 - Exceeds Expectations");

        // Assert
        Assert.Equal("5 - Exceeds Expectations", review.OverallRating);
    }

    [Fact]
    public void SetRating_WithInvalidRating_ThrowsValidationException()
    {
        // Arrange
        var review = PerformanceReview.Create(_employeeId, "Q3 2026", DateTime.Now, _tenantId);

        // Act & Assert
        Assert.Throws<ValidationException>(() => review.SetRating("Invalid Rating"));
    }

    [Fact]
    public void SubmitReview_FromDraft_ChangesStatusToSubmitted()
    {
        // Arrange
        var review = PerformanceReview.Create(_employeeId, "Q3 2026", DateTime.Now, _tenantId);
        review.SetRating("4 - Meets Expectations");
        Assert.Equal("Draft", review.Status);

        // Act
        review.SubmitReview(Guid.NewGuid());

        // Assert
        Assert.Equal("Submitted", review.Status);
        Assert.NotNull(review.SubmittedDate);
    }

    [Fact]
    public void SubmitReview_PublishesDomainEvent()
    {
        // Arrange
        var review = PerformanceReview.Create(_employeeId, "Q3 2026", DateTime.Now, _tenantId);
        review.SetRating("4 - Meets Expectations");

        // Act
        var reviewerId = Guid.NewGuid();
        review.SubmitReview(reviewerId);

        // Assert
        var submitEvent = review.DomainEvents.OfType<PerformanceReviewSubmittedEvent>().FirstOrDefault();
        Assert.NotNull(submitEvent);
        Assert.Equal(review.Id, submitEvent.ReviewId);
        Assert.Equal(_employeeId, submitEvent.EmployeeId);
        Assert.Equal(reviewerId, submitEvent.ReviewerId);
    }

    [Fact]
    public void ApproveReview_FromSubmitted_ChangesStatusToApproved()
    {
        // Arrange
        var review = PerformanceReview.Create(_employeeId, "Q3 2026", DateTime.Now, _tenantId);
        review.SetRating("4 - Meets Expectations");
        review.SubmitReview(Guid.NewGuid());
        Assert.Equal("Submitted", review.Status);

        // Act
        review.ApproveReview(Guid.NewGuid());

        // Assert
        Assert.Equal("Approved", review.Status);
        Assert.NotNull(review.ApprovedDate);
    }

    [Fact]
    public void ApproveReview_PublishesDomainEvent()
    {
        // Arrange
        var review = PerformanceReview.Create(_employeeId, "Q3 2026", DateTime.Now, _tenantId);
        review.SetRating("4 - Meets Expectations");
        review.SubmitReview(Guid.NewGuid());

        // Act
        var approverId = Guid.NewGuid();
        review.ApproveReview(approverId);

        // Assert
        var approveEvent = review.DomainEvents.OfType<PerformanceReviewApprovedEvent>().FirstOrDefault();
        Assert.NotNull(approveEvent);
        Assert.Equal(review.Id, approveEvent.ReviewId);
        Assert.Equal(approverId, approveEvent.ApproverId);
    }

    [Fact]
    public void RejectReview_FromSubmitted_ChangesStatusToRejected()
    {
        // Arrange
        var review = PerformanceReview.Create(_employeeId, "Q3 2026", DateTime.Now, _tenantId);
        review.SetRating("4 - Meets Expectations");
        review.SubmitReview(Guid.NewGuid());

        // Act
        review.RejectReview("Needs more feedback");

        // Assert
        Assert.Equal("Rejected", review.Status);
        Assert.Equal("Needs more feedback", review.RejectionReason);
    }

    [Fact]
    public void CanSubmit_WithRating_ReturnsTrue()
    {
        // Arrange
        var review = PerformanceReview.Create(_employeeId, "Q3 2026", DateTime.Now, _tenantId);
        review.SetRating("4 - Meets Expectations");

        // Act
        var canSubmit = review.CanSubmit();

        // Assert
        Assert.True(canSubmit);
    }

    [Fact]
    public void CanSubmit_WithoutRating_ReturnsFalse()
    {
        // Arrange
        var review = PerformanceReview.Create(_employeeId, "Q3 2026", DateTime.Now, _tenantId);

        // Act
        var canSubmit = review.CanSubmit();

        // Assert
        Assert.False(canSubmit);
    }

    [Fact]
    public void GetPerformanceReviewById_WithValidId_ReturnsReview()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var review = PerformanceReview.Create(_employeeId, "Q3 2026", DateTime.Now, _tenantId)
        {
            Id = reviewId
        };
        review.SetRating("4 - Meets Expectations");

        var reviews = new List<PerformanceReview> { review }.AsQueryable();

        // Act
        var result = reviews.FirstOrDefault(r => r.Id == reviewId && r.TenantId == _tenantId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(reviewId, result.Id);
        Assert.Equal(_employeeId, result.EmployeeId);
    }

    [Fact]
    public void GetEmployeeReviews_FiltersByEmployeeId_ReturnsList()
    {
        // Arrange
        var employee1Id = Guid.NewGuid();
        var employee2Id = Guid.NewGuid();

        var reviews = new List<PerformanceReview>
        {
            PerformanceReview.Create(employee1Id, "Q3 2026", DateTime.Now, _tenantId),
            PerformanceReview.Create(employee1Id, "Q2 2026", DateTime.Now.AddMonths(-3), _tenantId),
            PerformanceReview.Create(employee2Id, "Q3 2026", DateTime.Now, _tenantId)
        };

        // Act
        var employeeReviews = reviews.Where(r => r.EmployeeId == employee1Id && r.TenantId == _tenantId).ToList();

        // Assert
        Assert.Equal(2, employeeReviews.Count);
        Assert.All(employeeReviews, r => Assert.Equal(employee1Id, r.EmployeeId));
    }
}
