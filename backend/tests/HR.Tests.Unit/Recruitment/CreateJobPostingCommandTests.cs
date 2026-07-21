namespace HR.Tests.Unit.Recruitment;

public class CreateJobPostingCommandTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepository<JobPosting>> _mockRepository;
    private readonly CreateJobPostingCommandHandler _handler;

    public CreateJobPostingCommandTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockRepository = new Mock<IRepository<JobPosting>>();
        _handler = new CreateJobPostingCommandHandler(_mockUnitOfWork.Object, _mockRepository.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_CreatesJobPosting()
    {
        // Arrange
        var request = new CreateJobPostingRequest(
            Title: "Software Engineer",
            Description: "We are looking for an experienced software engineer to join our team.",
            Department: "Engineering",
            RequiredSkills: ["C#", "ASP.NET Core", "SQL"],
            SalaryMin: 80000,
            SalaryMax: 120000);

        var command = new CreateJobPostingCommand(request, Guid.NewGuid());

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<JobPosting>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Software Engineer", result.Title);
        Assert.Equal("Engineering", result.Department);
        Assert.Equal(3, result.RequiredSkills.Count);
        Assert.Equal("Draft", result.Status);
    }

    [Fact]
    public async Task Handle_WithMissingTitle_ThrowsDomainException()
    {
        // Arrange
        var request = new CreateJobPostingRequest(
            Title: "",
            Description: "Valid description with enough characters.",
            Department: "Engineering",
            RequiredSkills: ["C#"],
            SalaryMin: null,
            SalaryMax: null);

        var command = new CreateJobPostingCommand(request, Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithEmptySkills_ThrowsDomainException()
    {
        // Arrange
        var request = new CreateJobPostingRequest(
            Title: "Software Engineer",
            Description: "Valid description with enough characters.",
            Department: "Engineering",
            RequiredSkills: [],
            SalaryMin: null,
            SalaryMax: null);

        var command = new CreateJobPostingCommand(request, Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithInvalidSalaryRange_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateJobPostingRequest(
            Title: "Software Engineer",
            Description: "Valid description with enough characters.",
            Department: "Engineering",
            RequiredSkills: ["C#"],
            SalaryMin: 120000,
            SalaryMax: 80000); // Max less than min

        var command = new CreateJobPostingCommand(request, Guid.NewGuid());
        var validator = new CreateJobPostingCommandValidator();

        // Act
        var validationResult = await validator.ValidateAsync(command);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.Contains(validationResult.Errors, e => e.PropertyName.Contains("SalaryMax"));
    }
}
