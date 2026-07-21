namespace HR.Tests.Unit.Identity;

/// <summary>
/// Unit tests for LoginCommand handler.
/// Tests cover: valid login, invalid credentials, user not found, account lockout, and validation scenarios.
/// </summary>
public class LoginCommandTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepository<User>> _mockUserRepository;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly Mock<ILogger<LoginCommandHandler>> _mockLogger;
    private readonly LoginCommandHandler _handler;

    public LoginCommandTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserRepository = new Mock<IRepository<User>>();
        _mockTokenService = new Mock<ITokenService>();
        _mockLogger = new Mock<ILogger<LoginCommandHandler>>();

        _mockUnitOfWork
            .Setup(u => u.GetRepository<User>())
            .Returns(_mockUserRepository.Object);

        _handler = new LoginCommandHandler(
            _mockUnitOfWork.Object,
            _mockTokenService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsLoginResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var user = User.Create(
            email: "test@example.com",
            username: "testuser",
            fullName: "Test User",
            passwordHash: BCrypt.Net.BCrypt.HashPassword("ValidPassword123"),
            tenantId: tenantId);

        var role = Role.Create("Admin", "Administrator", tenantId, isSystemRole: true);
        user.AddRole(role);
        user.UserRoles.First().Role = role;

        var command = new LoginCommand("test@example.com", "ValidPassword123", false);

        var queryable = new List<User> { user }.AsQueryable();
        _mockUserRepository
            .Setup(r => r.GetAsQueryable())
            .Returns(queryable);

        _mockTokenService
            .Setup(t => t.GenerateAccessToken(It.IsAny<User>(), It.IsAny<List<string>>()))
            .Returns("test-token");

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal("Test User", result.FullName);
        Assert.Equal("test-token", result.AccessToken);
        Assert.NotNull(result.RefreshToken);
        Assert.Equal(3600, result.ExpiresIn);
        Assert.Contains("Admin", result.Roles);

        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ThrowsForbiddenException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var user = User.Create(
            email: "test@example.com",
            username: "testuser",
            fullName: "Test User",
            passwordHash: BCrypt.Net.BCrypt.HashPassword("CorrectPassword123"),
            tenantId: tenantId);

        var command = new LoginCommand("test@example.com", "WrongPassword123", false);

        var queryable = new List<User> { user }.AsQueryable();
        _mockUserRepository
            .Setup(r => r.GetAsQueryable())
            .Returns(queryable);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ForbiddenException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.Contains("Invalid email or password", exception.Message);
        Assert.Equal(1, user.LoginAttempts);

        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithUserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var command = new LoginCommand("nonexistent@example.com", "Password123", false);

        var queryable = new List<User>().AsQueryable();
        _mockUserRepository
            .Setup(r => r.GetAsQueryable())
            .Returns(queryable);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.Contains("User", exception.Message);
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ThrowsForbiddenException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var user = User.Create(
            email: "test@example.com",
            username: "testuser",
            fullName: "Test User",
            passwordHash: BCrypt.Net.BCrypt.HashPassword("ValidPassword123"),
            tenantId: tenantId);

        user.IsActive = false;

        var command = new LoginCommand("test@example.com", "ValidPassword123", false);

        var queryable = new List<User> { user }.AsQueryable();
        _mockUserRepository
            .Setup(r => r.GetAsQueryable())
            .Returns(queryable);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ForbiddenException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.Contains("inactive", exception.Message);
    }

    [Fact]
    public async Task Handle_WithLockedOutUser_ThrowsForbiddenException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var user = User.Create(
            email: "test@example.com",
            username: "testuser",
            fullName: "Test User",
            passwordHash: BCrypt.Net.BCrypt.HashPassword("ValidPassword123"),
            tenantId: tenantId);

        user.LockoutEndUtc = DateTime.UtcNow.AddMinutes(30);

        var command = new LoginCommand("test@example.com", "ValidPassword123", false);

        var queryable = new List<User> { user }.AsQueryable();
        _mockUserRepository
            .Setup(r => r.GetAsQueryable())
            .Returns(queryable);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ForbiddenException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.Contains("locked", exception.Message);
    }

    [Fact]
    public async Task Handle_WithMultipleFailedAttempts_LocksOutUser()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var user = User.Create(
            email: "test@example.com",
            username: "testuser",
            fullName: "Test User",
            passwordHash: BCrypt.Net.BCrypt.HashPassword("CorrectPassword123"),
            tenantId: tenantId);

        var command = new LoginCommand("test@example.com", "WrongPassword123", false);

        var queryable = new List<User> { user }.AsQueryable();
        _mockUserRepository
            .Setup(r => r.GetAsQueryable())
            .Returns(queryable);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act - Simulate 5 failed attempts
        for (int i = 0; i < 5; i++)
        {
            await Assert.ThrowsAsync<ForbiddenException>(
                () => _handler.Handle(command, CancellationToken.None));
        }

        // Assert
        Assert.Equal(5, user.LoginAttempts);
        Assert.True(user.IsLockedOut);
        Assert.NotNull(user.LockoutEndUtc);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ResetsLoginAttempts()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var user = User.Create(
            email: "test@example.com",
            username: "testuser",
            fullName: "Test User",
            passwordHash: BCrypt.Net.BCrypt.HashPassword("ValidPassword123"),
            tenantId: tenantId);

        // Simulate previous failed attempts
        user.RecordFailedLoginAttempt();
        user.RecordFailedLoginAttempt();
        Assert.Equal(2, user.LoginAttempts);

        var role = Role.Create("User", "Standard User", tenantId);
        user.AddRole(role);
        user.UserRoles.First().Role = role;

        var command = new LoginCommand("test@example.com", "ValidPassword123", false);

        var queryable = new List<User> { user }.AsQueryable();
        _mockUserRepository
            .Setup(r => r.GetAsQueryable())
            .Returns(queryable);

        _mockTokenService
            .Setup(t => t.GenerateAccessToken(It.IsAny<User>(), It.IsAny<List<string>>()))
            .Returns("test-token");

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, user.LoginAttempts);
        Assert.Null(user.LockoutEndUtc);
        Assert.NotNull(user.LastLoginUtc);
    }
}
