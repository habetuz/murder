using System.Collections.Concurrent;
using System.Security.Cryptography;
using Murder.DomainGame;
using Murder.DomainIdentity;

namespace Murder.Plugins.WebAPI;

public sealed class RestoreTokenStore
{
    private readonly ConcurrentDictionary<string, RestoreToken> _tokens = new();
    private readonly TimeSpan _tokenLifetime = TimeSpan.FromMinutes(15);

    public string Generate(IdentityId identityId, GameId gameId)
    {
        var token = GenerateSecureToken();
        var restoreToken = new RestoreToken(identityId, gameId, DateTimeOffset.UtcNow + _tokenLifetime);
        _tokens[token] = restoreToken;
        return token;
    }

    public RestoreToken? Redeem(string token)
    {
        if (!_tokens.TryRemove(token, out var restoreToken))
        {
            return null;
        }

        if (restoreToken.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            return null;
        }

        return restoreToken;
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(randomBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}

public sealed record RestoreToken(IdentityId IdentityId, GameId GameId, DateTimeOffset ExpiresAtUtc);
