using Murder.DomainIdentity;

namespace Murder.Adapters.AuthenticationMethod.SessionToken;

public readonly record struct SessionTokenIncomingCredential(string Token)
    : IIncomingCredential<SessionTokenMethodKey>;
