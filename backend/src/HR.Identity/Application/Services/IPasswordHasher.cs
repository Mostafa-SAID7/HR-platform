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

/// <summary>
/// Bcrypt-based password hasher implementation.
/// </summary>
public class BcryptPasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }
}
