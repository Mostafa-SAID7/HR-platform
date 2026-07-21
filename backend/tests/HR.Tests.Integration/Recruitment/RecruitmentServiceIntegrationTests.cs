namespace HR.Tests.Integration.Recruitment;

public class RecruitmentServiceIntegrationTests : IAsyncLifetime
{
    private PostgreSqlFixture _dbFixture;
    private IServiceProvider _serviceProvider;

    public async Task InitializeAsync()
    {
        _dbFixture = new PostgreSqlFixture("hr_recruitment_test");
        await _dbFixture.InitializeAsync();

        var services = new ServiceCollection();
        services.AddDbContext<RecruitmentDbContext>(options =>
            options.UseNpgsql(_dbFixture.ConnectionString));
        services.AddScoped<IUnitOfWork, RecruitmentDbContext>();
        services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));

        _serviceProvider = services.BuildServiceProvider();

        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<RecruitmentDbContext>();
            await context.Database.MigrateAsync();
        }
    }

    [Fact]
    public async Task CreateJobPosting_WithValidData_PersistsToDatabase()
    {
        // Arrange
        using (var scope = _serviceProvider.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRepository<JobPosting>>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var jobPosting = JobPosting.Create(
                "Software Engineer",
                "We are seeking an experienced software engineer with expertise in cloud technologies.",
                "Engineering",
                ["C#", "ASP.NET Core", "Azure"],
                90000,
                130000);

            // Act
            await repository.AddAsync(jobPosting);
            await unitOfWork.SaveChangesAsync();

            // Assert
            var retrieved = await repository.GetByIdAsync(jobPosting.Id);
            Assert.NotNull(retrieved);
            Assert.Equal("Software Engineer", retrieved.Title);
            Assert.Equal(JobPostingStatus.Draft, retrieved.Status);
        }
    }

    [Fact]
    public async Task PublishJobPosting_ChangesStatusToOpen()
    {
        // Arrange
        using (var scope = _serviceProvider.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRepository<JobPosting>>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var jobPosting = JobPosting.Create(
                "Senior Developer",
                "Seeking a senior developer to lead our engineering team.",
                "Engineering",
                ["C#", "Leadership"],
                120000,
                150000);

            await repository.AddAsync(jobPosting);
            await unitOfWork.SaveChangesAsync();

            // Act
            jobPosting.Publish();
            await unitOfWork.SaveChangesAsync();

            // Assert
            var retrieved = await repository.GetByIdAsync(jobPosting.Id);
            Assert.NotNull(retrieved);
            Assert.Equal(JobPostingStatus.Open, retrieved.Status);
        }
    }

    [Fact]
    public async Task ApplyForJob_CreatesJobApplication()
    {
        // Arrange
        using (var scope = _serviceProvider.CreateScope())
        {
            var jobPostingRepo = scope.ServiceProvider.GetRequiredService<IRepository<JobPosting>>();
            var appRepo = scope.ServiceProvider.GetRequiredService<IRepository<JobApplication>>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var jobPosting = JobPosting.Create(
                "Product Manager",
                "Looking for a talented product manager.",
                "Product",
                ["Product Strategy"],
                100000,
                140000);

            jobPosting.Publish();
            await jobPostingRepo.AddAsync(jobPosting);
            await unitOfWork.SaveChangesAsync();

            var candidateId = Guid.NewGuid();

            // Act
            var application = JobApplication.Create(
                jobPosting.Id,
                candidateId,
                "Jane Smith",
                "jane@example.com",
                "+1-555-0100",
                "resume_jane_smith.pdf",
                "I am very interested in this position...");

            await appRepo.AddAsync(application);
            await unitOfWork.SaveChangesAsync();

            // Assert
            var retrieved = await appRepo.GetByIdAsync(application.Id);
            Assert.NotNull(retrieved);
            Assert.Equal("Jane Smith", retrieved.CandidateName);
            Assert.Equal(ApplicationStatus.Submitted, retrieved.Status);
        }
    }

    [Fact]
    public async Task ScheduleInterview_CreatesInterviewSchedule()
    {
        // Arrange
        using (var scope = _serviceProvider.CreateScope())
        {
            var jobPostingRepo = scope.ServiceProvider.GetRequiredService<IRepository<JobPosting>>();
            var appRepo = scope.ServiceProvider.GetRequiredService<IRepository<JobApplication>>();
            var interviewRepo = scope.ServiceProvider.GetRequiredService<IRepository<InterviewSchedule>>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var jobPosting = JobPosting.Create(
                "QA Engineer",
                "Quality assurance engineer needed.",
                "QA",
                ["Testing"],
                70000,
                100000);

            jobPosting.Publish();
            await jobPostingRepo.AddAsync(jobPosting);
            await unitOfWork.SaveChangesAsync();

            var application = JobApplication.Create(
                jobPosting.Id,
                Guid.NewGuid(),
                "Bob Wilson",
                "bob@example.com",
                "+1-555-0101",
                "resume_bob.pdf");

            await appRepo.AddAsync(application);
            await unitOfWork.SaveChangesAsync();

            // Act
            var scheduledDate = DateTime.UtcNow.AddDays(5);
            var endTime = scheduledDate.AddHours(1);

            var interview = InterviewSchedule.Create(
                application.Id,
                scheduledDate,
                endTime,
                "Video",
                "Alice Johnson",
                "alice@example.com",
                "https://meet.example.com/interview123",
                null);

            await interviewRepo.AddAsync(interview);
            await unitOfWork.SaveChangesAsync();

            // Assert
            var retrieved = await interviewRepo.GetByIdAsync(interview.Id);
            Assert.NotNull(retrieved);
            Assert.Equal(InterviewStatus.Scheduled, retrieved.Status);
            Assert.Equal("Video", retrieved.InterviewType);
        }
    }

    [Fact]
    public async Task CreateOfferLetter_WithValidData_StoresOffer()
    {
        // Arrange
        using (var scope = _serviceProvider.CreateScope())
        {
            var jobPostingRepo = scope.ServiceProvider.GetRequiredService<IRepository<JobPosting>>();
            var appRepo = scope.ServiceProvider.GetRequiredService<IRepository<JobApplication>>();
            var offerRepo = scope.ServiceProvider.GetRequiredService<IRepository<OfferLetter>>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var jobPosting = JobPosting.Create(
                "Data Analyst",
                "Data analyst position available.",
                "Analytics",
                ["SQL"],
                75000,
                110000);

            jobPosting.Publish();
            await jobPostingRepo.AddAsync(jobPosting);
            await unitOfWork.SaveChangesAsync();

            var candidateId = Guid.NewGuid();
            var application = JobApplication.Create(
                jobPosting.Id,
                candidateId,
                "Carol Davis",
                "carol@example.com",
                "+1-555-0102",
                "resume_carol.pdf");

            await appRepo.AddAsync(application);
            await unitOfWork.SaveChangesAsync();

            // Act
            var offer = OfferLetter.Create(
                application.Id,
                candidateId,
                95000,
                "Analytics",
                "Senior Data Analyst",
                DateTime.UtcNow.AddDays(30));

            await offerRepo.AddAsync(offer);
            await unitOfWork.SaveChangesAsync();

            // Assert
            var retrieved = await offerRepo.GetByIdAsync(offer.Id);
            Assert.NotNull(retrieved);
            Assert.Equal(95000, retrieved.OfferSalary);
            Assert.Equal(OfferStatus.Pending, retrieved.Status);
        }
    }

    public async Task DisposeAsync()
    {
        await _dbFixture.DisposeAsync();
    }
}
