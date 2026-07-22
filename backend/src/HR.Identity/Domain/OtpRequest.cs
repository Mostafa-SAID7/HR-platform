namespace HR.Identity.Domain;

/// <summary>
/// OTP (One-Time Password) request entity for email/phone verification
/// </summary>
public class OtpRequest : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public string OtpCode { get; private set; } = string.Empty;
    public OtpType OtpType { get; private set; }
    public DateTime ExpiryTime { get; private set; }
    public int AttemptCount { get; private set; }
    public int MaxAttempts { get; private set; } = 3;
    public bool IsUsed { get; private set; }
    public DateTime? UsedAt { get; private set; }
    public User? User { get; private set; }

    private OtpRequest() { }

    /// <summary>
    /// Create a new OTP request
    /// </summary>
    public static OtpRequest Create(
        Guid userId,
        string email,
        string? phoneNumber,
        OtpType otpType,
        Guid tenantId,
        int expiryMinutes = 5)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required for OTP");

        var otpRequest = new OtpRequest
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Email = email,
            PhoneNumber = phoneNumber,
            OtpCode = GenerateOtpCode(),
            OtpType = otpType,
            ExpiryTime = DateTime.UtcNow.AddMinutes(expiryMinutes),
            AttemptCount = 0,
            IsUsed = false,
            TenantId = tenantId,
            CreatedOnUtc = DateTime.UtcNow
        };

        return otpRequest;
    }

    /// <summary>
    /// Verify OTP code
    /// </summary>
    public bool VerifyOtp(string code)
    {
        if (IsUsed)
            return false;

        if (DateTime.UtcNow > ExpiryTime)
            return false;

        if (AttemptCount >= MaxAttempts)
            return false;

        AttemptCount++;

        if (OtpCode == code)
        {
            IsUsed = true;
            UsedAt = DateTime.UtcNow;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Check if OTP is expired
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > ExpiryTime;

    /// <summary>
    /// Check if OTP verification is still allowed
    /// </summary>
    public bool CanVerify => !IsUsed && !IsExpired && AttemptCount < MaxAttempts;

    /// <summary>
    /// Generate random OTP code
    /// </summary>
    private static string GenerateOtpCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}

/// <summary>
/// OTP type enumeration
/// </summary>
public enum OtpType
{
    EmailVerification = 0,
    PhoneVerification = 1,
    PasswordReset = 2,
    TwoFactorAuthentication = 3,
    AccountRecovery = 4
}
