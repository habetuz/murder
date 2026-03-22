using Murder.DomainIdentity;

namespace Murder.Plugins.AuthenticationMethod.SessionToken;

public readonly record struct SessionTokenIncomingCredential(string Token)
    : IIncomingCredential<SessionTokenMethodKey>;
