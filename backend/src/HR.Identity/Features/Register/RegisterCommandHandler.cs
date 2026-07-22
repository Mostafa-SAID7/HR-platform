namespace HR.Identity.Features.Register;

using HR.Identity.Domain;
using HR.Identity.Application.Services;
using HR.Identity.Application.Dtos.Register;
using HR.Identity.Application.Mappings;
using HR.Common;

/// <summary>
/// Handler for RegisterCommand
/// </summary>
public class RegisterCommandHandler : ICommandHandler<RegisterCommand, RegisterResponse>
{
    private readonly IRepository<User> _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IRepository<User> userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        ILogger<RegisterCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Registering new user: {Username}", request.Request.Username);

        // Check if user already exists
        var existingUser = await _userRepository.FirstOrDefaultAsync(
            u => u.Email == request.Request.Email || u.Username == request.Request.Username,
            cancellationToken);

        if (existingUser != null)
        {
            throw new ValidationException("User with this email or username already exists");
        }

        // Hash password
        var passwordHash = _passwordHasher.HashPassword(request.Request.Password);

        // Create new user
        var user = User.Create(
            request.Request.Email,
            request.Request.Username,
            request.Request.FullName,
            passwordHash,
            request.TenantId);

        // Add user to repository
        await _userRepository.AddAsync(user, cancellationToken);

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User registered successfully: {UserId}", user.Id);

        // Map to response
        return user.ToRegisterResponse();
    }
}
