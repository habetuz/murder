using System.Security.Cryptography;
using System.Text;
using Murder.DomainIdentity;

namespace Murder.Plugins.AuthenticationMethod.SessionToken;

public class SessionTokenAuthenticationMethod(
    ICredentialRepository credentials,
    TimeProvider? timeProvider = null,
    TimeSpan? tokenLifetime = null
) : IAuthenticationMethod<
    SessionTokenIncomingCredential,
    SessionTokenEnrollmentData,
    SessionTokenMethodKey
>
{
    private readonly ICredentialRepository _credentials = credentials;
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;
    private readonly TimeSpan _tokenLifetime =
        tokenLifetime is { } configuredLifetime && configuredLifetime > TimeSpan.Zero
            ? configuredLifetime
            : TimeSpan.FromMinutes(30);

    public IdentityId? Authenticate(SessionTokenIncomingCredential credential)
    {
        if (string.IsNullOrWhiteSpace(credential.Token))
        {
            return null;
        }

        var now = _timeProvider.GetUtcNow();
        var ids = _credentials.FindAll<SessionTokenMethodKey>();

        foreach (var id in ids)
        {
            var stored = _credentials.FindById<SessionTokenMethodKey>(id);
            if (stored is null)
            {
                continue;
            }

            if (stored.Value.credential is not SessionTokenStoredCredential storedCredential)
            {
                continue;
            }

            if (storedCredential.ExpiresAtUtc <= now)
            {
                _credentials.Delete(id);
                continue;
            }

            if (!VerifyToken(credential.Token, storedCredential.TokenHashBase64))
            {
                continue;
            }

            // Sliding expiration: every successful authenticate extends the token lifetime.
            _credentials.Update(
                id,
                new SessionTokenStoredCredential(HashToken(credential.Token), now + _tokenLifetime)
            );

            return stored.Value.identity;
        }

        return null;
    }

    public EnrollmentResult<SessionTokenMethodKey> Enroll(SessionTokenEnrollmentData enrollmentData)
    {
        var token = GenerateSecureToken();
        var storedCredential = new SessionTokenStoredCredential(
            HashToken(token),
            _timeProvider.GetUtcNow() + _tokenLifetime
        );

        return new EnrollmentResult<SessionTokenMethodKey>(storedCredential, token);
    }

    public int Cleanup()
    {
        var now = _timeProvider.GetUtcNow();
        var deleted = 0;

        var ids = _credentials.FindAll<SessionTokenMethodKey>();

        foreach (var id in ids)
        {
            var stored = _credentials.FindById<SessionTokenMethodKey>(id);
            if (stored is null)
            {
                continue;
            }

            if (stored.Value.credential is not SessionTokenStoredCredential storedCredential)
            {
                continue;
            }

            if (storedCredential.ExpiresAtUtc <= now)
            {
                _credentials.Delete(id);
                deleted++;
            }
        }

        return deleted;
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(randomBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string HashToken(string token)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var hashBytes = SHA256.HashData(tokenBytes);
        return Convert.ToBase64String(hashBytes);
    }

    private static bool VerifyToken(string providedToken, string expectedHashBase64)
    {
        byte[] expectedHash;

        try
        {
            expectedHash = Convert.FromBase64String(expectedHashBase64);
        }
        catch (FormatException)
        {
            return false;
        }

        var providedHash = SHA256.HashData(Encoding.UTF8.GetBytes(providedToken));
        return CryptographicOperations.FixedTimeEquals(expectedHash, providedHash);
    }
}
