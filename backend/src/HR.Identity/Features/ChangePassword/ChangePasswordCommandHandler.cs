namespace HR.Identity.Features.ChangePassword;

using HR.Identity.Domain;
using HR.Identity.Application.Services;
using HR.Identity.Application.Dtos.UserProfile;
using HR.Common;

/// <summary>
/// Handler for ChangePasswordCommand
/// </summary>
public class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand, ChangePasswordResponse>
{
    private readonly IRepository<User> _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ChangePasswordCommandHandler> _logger;

    public ChangePasswordCommandHandler(
        IRepository<User> userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        ILogger<ChangePasswordCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ChangePasswordResponse> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Changing password for user: {UserId}", request.UserId);

        // Get user
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new ValidationException("User not found");
        }

        // Verify current password
        var isPasswordValid = _passwordHasher.VerifyPassword(request.Request.CurrentPassword, user.PasswordHash);
        if (!isPasswordValid)
        {
            throw new ValidationException("Current password is incorrect");
        }

        // Hash new password
        var newPasswordHash = _passwordHasher.HashPassword(request.Request.NewPassword);

        // Update password
        user.PasswordHash = newPasswordHash;

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Password changed successfully for user: {UserId}", request.UserId);

        return new ChangePasswordResponse(true, "Password changed successfully");
    }
}
