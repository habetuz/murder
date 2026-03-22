using Murder.DomainIdentity;

namespace Murder.Plugins.AuthenticationMethod.SessionToken;

public readonly record struct SessionTokenEnrollmentData(IdentityId IdentityId)
    : IEnrollmentData<SessionTokenMethodKey>;
