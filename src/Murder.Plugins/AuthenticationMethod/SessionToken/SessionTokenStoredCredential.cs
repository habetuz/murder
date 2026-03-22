using Murder.DomainIdentity;

namespace Murder.Plugins.AuthenticationMethod.SessionToken;

public readonly record struct SessionTokenStoredCredential(string TokenHashBase64, DateTimeOffset ExpiresAtUtc)
    : IStoredCredential<SessionTokenMethodKey>;
