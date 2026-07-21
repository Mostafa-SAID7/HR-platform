namespace HR.Tests.Integration.Performance;

/// <summary>
/// Integration tests for Performance Service (11 tests).
/// Tests cover: CRUD, workflow state transitions, event publishing, and queries.
/// </summary>
[Collection("Database collection")]
public class PerformanceServiceIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<PerformanceReview> _reviewRepository;

    public PerformanceServiceIntegrationTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
        _unitOfWork = _fixture.CreateUnitOfWork();
        _reviewRepository = _unitOfWork.GetRepository<PerformanceReview>();
    }

    public async Task InitializeAsync() => await _fixture.InitializeAsync();
    public async Task DisposeAsync() => await _fixture.DisposeAsync();

    [Fact]
    public async Task CreatePerformanceReview_PersistsToDatabase()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var review = PerformanceReview.Create(employeeId, "Q3 2026", DateTime.Now, tenantId);

        // Act
        await _reviewRepository.AddAsync(review);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var retrieved = await _reviewRepository.GetByIdAsync(review.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(employeeId, retrieved.EmployeeId);
        Assert.Equal("Draft", retrieved.Status);
    }

    [Fact]
    public async Task SubmitReview_TransitionsStatus()
    {
        // Arrange
        var review = PerformanceReview.Create(Guid.NewGuid(), "Q3 2026", DateTime.Now, Guid.NewGuid());
        review.SetRating("4 - Meets Expectations");
        await _reviewRepository.AddAsync(review);
        await _unitOfWork.SaveChangesAsync();

        // Act
        review.SubmitReview(Guid.NewGuid());
        _reviewRepository.Update(review);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var retrieved = await _reviewRepository.GetByIdAsync(review.Id);
        Assert.Equal("Submitted", retrieved.Status);
        Assert.NotNull(retrieved.SubmittedDate);
    }

    [Fact]
    public async Task ApproveReview_TransitionsStatus()
    {
        // Arrange
        var review = PerformanceReview.Create(Guid.NewGuid(), "Q3 2026", DateTime.Now, Guid.NewGuid());
        review.SetRating("4 - Meets Expectations");
        review.SubmitReview(Guid.NewGuid());
        await _reviewRepository.AddAsync(review);
        await _unitOfWork.SaveChangesAsync();

        // Act
        review.ApproveReview(Guid.NewGuid());
        _reviewRepository.Update(review);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var retrieved = await _reviewRepository.GetByIdAsync(review.Id);
        Assert.Equal("Approved", retrieved.Status);
        Assert.NotNull(retrieved.ApprovedDate);
    }

    [Fact]
    public async Task AddFeedback_PersistsFeedback()
    {
        // Arrange
        var review = PerformanceReview.Create(Guid.NewGuid(), "Q3 2026", DateTime.Now, Guid.NewGuid());
        await _reviewRepository.AddAsync(review);
        await _unitOfWork.SaveChangesAsync();

        // Act
        review.AddFeedback("Communication", "Excellent");
        review.AddFeedback("Technical Skills", "Very Strong");
        _reviewRepository.Update(review);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var retrieved = await _reviewRepository.GetByIdAsync(review.Id);
        Assert.Equal(2, retrieved.Feedback.Count);
    }

    [Fact]
    public async Task RejectReview_SetsRejectionReason()
    {
        // Arrange
        var review = PerformanceReview.Create(Guid.NewGuid(), "Q3 2026", DateTime.Now, Guid.NewGuid());
        review.SetRating("4 - Meets Expectations");
        review.SubmitReview(Guid.NewGuid());
        await _reviewRepository.AddAsync(review);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var rejectionReason = "Needs more detailed feedback";
        review.RejectReview(rejectionReason);
        _reviewRepository.Update(review);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var retrieved = await _reviewRepository.GetByIdAsync(review.Id);
        Assert.Equal("Rejected", retrieved.Status);
        Assert.Equal(rejectionReason, retrieved.RejectionReason);
    }

    [Fact]
    public async Task GetEmployeeReviews_FiltersByEmployee()
    {
        // Arrange
        var employee1Id = Guid.NewGuid();
        var employee2Id = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var review1 = PerformanceReview.Create(employee1Id, "Q3 2026", DateTime.Now, tenantId);
        var review2 = PerformanceReview.Create(employee1Id, "Q2 2026", DateTime.Now.AddMonths(-3), tenantId);
        var review3 = PerformanceReview.Create(employee2Id, "Q3 2026", DateTime.Now, tenantId);

        await _reviewRepository.AddAsync(review1);
        await _reviewRepository.AddAsync(review2);
        await _reviewRepository.AddAsync(review3);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var queryable = _reviewRepository.GetAsQueryable();
        var employee1Reviews = queryable
            .Where(r => r.EmployeeId == employee1Id && r.TenantId == tenantId)
            .ToList();

        // Assert
        Assert.Equal(2, employee1Reviews.Count);
    }

    [Fact]
    public async Task GetPendingApprovals_FiltersByStatus()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var submitted = PerformanceReview.Create(Guid.NewGuid(), "Q3 2026", DateTime.Now, tenantId);
        submitted.SetRating("4 - Meets Expectations");
        submitted.SubmitReview(Guid.NewGuid());

        var approved = PerformanceReview.Create(Guid.NewGuid(), "Q3 2026", DateTime.Now, tenantId);
        approved.SetRating("4 - Meets Expectations");
        approved.SubmitReview(Guid.NewGuid());
        approved.ApproveReview(Guid.NewGuid());

        await _reviewRepository.AddAsync(submitted);
        await _reviewRepository.AddAsync(approved);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var queryable = _reviewRepository.GetAsQueryable();
        var pending = queryable
            .Where(r => r.TenantId == tenantId && r.Status == "Submitted")
            .ToList();

        // Assert
        Assert.Single(pending);
    }

    [Fact]
    public async Task DomainEvent_PerformanceReviewSubmitted_IsRaised()
    {
        // Arrange
        var review = PerformanceReview.Create(Guid.NewGuid(), "Q3 2026", DateTime.Now, Guid.NewGuid());
        review.SetRating("4 - Meets Expectations");

        // Act
        review.SubmitReview(Guid.NewGuid());
        await _reviewRepository.AddAsync(review);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        Assert.NotEmpty(review.DomainEvents);
        var submitEvent = review.DomainEvents.OfType<PerformanceReviewSubmittedEvent>().FirstOrDefault();
        Assert.NotNull(submitEvent);
    }

    [Fact]
    public async Task BulkInsertReviews_PerformsEfficiently()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var reviews = Enumerable.Range(1, 50)
            .Select(i => PerformanceReview.Create(
                Guid.NewGuid(), $"Period{i}", DateTime.Now, tenantId))
            .ToList();

        // Act
        foreach (var review in reviews)
        {
            await _reviewRepository.AddAsync(review);
        }
        var startTime = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        var duration = DateTime.UtcNow - startTime;

        // Assert
        Assert.True(duration.TotalSeconds < 5);
        var queryable = _reviewRepository.GetAsQueryable();
        var count = queryable.Count(r => r.TenantId == tenantId);
        Assert.Equal(50, count);
    }

    [Fact]
    public async Task QueryReviewsByRating_FiltersByCriteria()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var excellent = PerformanceReview.Create(Guid.NewGuid(), "Q3 2026", DateTime.Now, tenantId);
        excellent.SetRating("5 - Exceeds Expectations");

        var meets = PerformanceReview.Create(Guid.NewGuid(), "Q3 2026", DateTime.Now, tenantId);
        meets.SetRating("4 - Meets Expectations");

        await _reviewRepository.AddAsync(excellent);
        await _reviewRepository.AddAsync(meets);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var queryable = _reviewRepository.GetAsQueryable();
        var highRaters = queryable
            .Where(r => r.TenantId == tenantId && r.OverallRating.Contains("5 -"))
            .ToList();

        // Assert
        Assert.Single(highRaters);
    }
}
