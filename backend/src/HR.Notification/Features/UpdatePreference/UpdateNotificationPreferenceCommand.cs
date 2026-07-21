namespace HR.Notification.Features.UpdatePreference;

/// <summary>
/// Update notification preferences for a user
/// </summary>
public record UpdateNotificationPreferenceCommand(
    Guid UserId,
    UpdateNotificationPreferenceRequest Request,
    Guid TenantId) : ICommand<NotificationPreferenceDto>;

/// <summary>
/// Validator for UpdateNotificationPreferenceCommand
/// </summary>
public class UpdateNotificationPreferenceCommandValidator : AbstractValidator<UpdateNotificationPreferenceCommand>
{
    public UpdateNotificationPreferenceCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}

/// <summary>
/// Handler for UpdateNotificationPreferenceCommand
/// </summary>
public class UpdateNotificationPreferenceCommandHandler : ICommandHandler<UpdateNotificationPreferenceCommand, NotificationPreferenceDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<NotificationPreference> _preferenceRepository;
    private readonly ILogger<UpdateNotificationPreferenceCommandHandler> _logger;

    public UpdateNotificationPreferenceCommandHandler(
        IUnitOfWork unitOfWork,
        IRepository<NotificationPreference> preferenceRepository,
        ILogger<UpdateNotificationPreferenceCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _preferenceRepository = preferenceRepository;
        _logger = logger;
    }

    public async Task<NotificationPreferenceDto> Handle(UpdateNotificationPreferenceCommand request, CancellationToken cancellationToken)
    {
        // Get or create preference
        var spec = new PreferenceByUserIdSpecification(request.UserId);
        var preference = await _preferenceRepository.GetAsync(spec, cancellationToken);

        if (preference == null)
        {
            preference = new NotificationPreference
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                EmailEnabled = request.Request.EmailEnabled,
                SmsEnabled = request.Request.SmsEnabled,
                PushEnabled = request.Request.PushEnabled,
                InAppEnabled = request.Request.InAppEnabled,
                CreatedAt = DateTime.UtcNow
            };

            await _preferenceRepository.AddAsync(preference, cancellationToken);
        }
        else
        {
            preference.EmailEnabled = request.Request.EmailEnabled;
            preference.SmsEnabled = request.Request.SmsEnabled;
            preference.PushEnabled = request.Request.PushEnabled;
            preference.InAppEnabled = request.Request.InAppEnabled;
            preference.LastModifiedAt = DateTime.UtcNow;

            await _preferenceRepository.UpdateAsync(preference, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Notification preferences updated for user {UserId}", request.UserId);

        return preference.Adapt<NotificationPreferenceDto>();
    }
}

/// <summary>
/// Specification for getting preferences by user ID
/// </summary>
public class PreferenceByUserIdSpecification : Specification<NotificationPreference>
{
    public PreferenceByUserIdSpecification(Guid userId)
    {
        AddCriteria(x => x.UserId == userId);
    }
}
