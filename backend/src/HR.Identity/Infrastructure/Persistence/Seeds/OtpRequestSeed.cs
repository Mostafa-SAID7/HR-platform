namespace HR.Identity.Infrastructure.Persistence.Seeds;

using Microsoft.EntityFrameworkCore;
using HR.Identity.Domain;

/// <summary>
/// Seed OTP request test data
/// </summary>
public static class OtpRequestSeed
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        var tenantId = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479");
        var userId = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d480");

        var otpRequests = new[]
        {
            new OtpRequest
            {
                Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d481"),
                UserId = userId,
                Email = "m.ssaid356@gmail.com",
                PhoneNumber = "+1234567890",
                OtpCode = "123456",
                OtpType = OtpType.EmailVerification,
                IsUsed = true,
                AttemptCount = 1,
                MaxAttempts = 3,
                CreatedOnUtc = DateTime.UtcNow.AddHours(-1),
                ExpiryTime = DateTime.UtcNow.AddMinutes(4),
                UsedAt = DateTime.UtcNow.AddMinutes(-55),
                TenantId = tenantId
            },
            new OtpRequest
            {
                Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d482"),
                UserId = userId,
                Email = "m.ssaid356@gmail.com",
                PhoneNumber = "+1234567890",
                OtpCode = "654321",
                OtpType = OtpType.PhoneVerification,
                IsUsed = false,
                AttemptCount = 0,
                MaxAttempts = 3,
                CreatedOnUtc = DateTime.UtcNow.AddMinutes(-5),
                ExpiryTime = DateTime.UtcNow,
                UsedAt = null,
                TenantId = tenantId
            },
            new OtpRequest
            {
                Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d483"),
                UserId = userId,
                Email = "m.ssaid356@gmail.com",
                PhoneNumber = null,
                OtpCode = "789012",
                OtpType = OtpType.TwoFactorAuthentication,
                IsUsed = false,
                AttemptCount = 2,
                MaxAttempts = 3,
                CreatedOnUtc = DateTime.UtcNow.AddMinutes(-3),
                ExpiryTime = DateTime.UtcNow.AddMinutes(2),
                UsedAt = null,
                TenantId = tenantId
            }
        };

        modelBuilder.Entity<OtpRequest>().HasData(otpRequests);
    }
}
