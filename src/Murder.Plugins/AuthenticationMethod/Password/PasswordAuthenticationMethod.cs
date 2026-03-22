using System.Security.Cryptography;
using Murder.DomainIdentity;

namespace Murder.Plugins.AuthenticationMethod.Password;

public class PasswordAuthenticationMethod(ICredentialRepository credentials)
    : IAuthenticationMethod<PasswordIncomingCredential, PasswordEnrollmentData, PasswordMethodKey>
{
    private const int SaltLength = 16;
    private const int HashLength = 32;
    private const int Iterations = 100_000;

    public IdentityId? Authenticate(PasswordIncomingCredential credential)
    {
        if (string.IsNullOrWhiteSpace(credential.Password))
            return null;

        var ids = credentials.FindAll<PasswordMethodKey>(credential.IdentityId);

        foreach (var id in ids)
        {
            var stored = credentials.FindById<PasswordMethodKey>(id);
            if (stored is null)
                continue;

            if (VerifyPassword(credential.Password, stored.Value.credential))
                return stored.Value.identity;
        }

        return null;
    }

    public EnrollmentResult<PasswordMethodKey> Enroll(PasswordEnrollmentData enrollmentData)
    {
        if (string.IsNullOrWhiteSpace(enrollmentData.Password))
            throw new ArgumentException("Password must not be empty.", nameof(enrollmentData));

        var storedCredential = HashPassword(enrollmentData.Password);
        return new EnrollmentResult<PasswordMethodKey>(storedCredential, null);
    }

    private static PasswordStoredCredential HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltLength);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashLength);

        return new PasswordStoredCredential(
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash),
            Iterations
        );
    }

    private static bool VerifyPassword(string password, IStoredCredential<PasswordMethodKey> storedCredential)
    {
        if (storedCredential is not PasswordStoredCredential stored)
            return false;

        var salt = Convert.FromBase64String(stored.SaltBase64);
        var expectedHash = Convert.FromBase64String(stored.HashBase64);

        var providedHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            stored.Iterations,
            HashAlgorithmName.SHA256,
            expectedHash.Length
        );

        return CryptographicOperations.FixedTimeEquals(expectedHash, providedHash);
    }
}
