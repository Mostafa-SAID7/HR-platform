namespace HR.Identity.Application.Services;

/// <summary>
/// Interface for password hashing and verification.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hash a password.
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Verify a password against its hash.
    /// </summary>
    bool VerifyPassword(string password, string hash);
}
