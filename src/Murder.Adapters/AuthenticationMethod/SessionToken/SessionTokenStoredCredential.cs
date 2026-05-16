using Murder.DomainIdentity;

namespace Murder.Adapters.AuthenticationMethod.SessionToken;

public readonly record struct SessionTokenStoredCredential(string TokenHashBase64, DateTimeOffset ExpiresAtUtc)
    : IStoredCredential<SessionTokenMethodKey>;
