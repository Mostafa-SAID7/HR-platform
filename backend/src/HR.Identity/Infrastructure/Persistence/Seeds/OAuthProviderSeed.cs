namespace HR.Identity.Infrastructure.Persistence.Seeds;

using Microsoft.EntityFrameworkCore;
using HR.Identity.Domain;

/// <summary>
/// Seed OAuth provider test data
/// </summary>
public static class OAuthProviderSeed
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        var tenantId = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479");
        var userId = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d480");

        var oauthProviders = new[]
        {
            new OAuthProvider
            {
                Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d484"),
                UserId = userId,
                ProviderType = OAuthProviderType.Google,
                ProviderUserId = "118364757813476952619",
                ProviderEmail = "m.ssaid356@gmail.com",
                ProviderName = "Google",
                ProfilePictureUrl = "https://lh3.googleusercontent.com/a/default-user=s96-c",
                AccessToken = "ya29.example_access_token",
                RefreshToken = null,
                TokenExpiry = DateTime.UtcNow.AddHours(1),
                IsActive = true,
                ConnectedAt = DateTime.UtcNow.AddDays(-7),
                TenantId = tenantId,
                CreatedOnUtc = DateTime.UtcNow.AddDays(-7),
                LastModifiedAt = DateTime.UtcNow.AddHours(-2)
            },
            new OAuthProvider
            {
                Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d485"),
                UserId = userId,
                ProviderType = OAuthProviderType.Facebook,
                ProviderUserId = "1234567890123456",
                ProviderEmail = "m.ssaid356@gmail.com",
                ProviderName = "Facebook",
                ProfilePictureUrl = "https://platform-lookaside.fbsbx.com/platform/profilepic/example",
                AccessToken = "EAABSBJ6xRH0BAExample",
                RefreshToken = null,
                TokenExpiry = DateTime.UtcNow.AddDays(60),
                IsActive = true,
                ConnectedAt = DateTime.UtcNow.AddDays(-5),
                TenantId = tenantId,
                CreatedOnUtc = DateTime.UtcNow.AddDays(-5),
                LastModifiedAt = DateTime.UtcNow.AddHours(-24)
            }
        };

        modelBuilder.Entity<OAuthProvider>().HasData(oauthProviders);
    }
}
