using Murder.DomainIdentity;

namespace Murder.Adapters.AuthenticationMethod.SessionToken;

public readonly record struct SessionTokenEnrollmentData(IdentityId IdentityId)
    : IEnrollmentData<SessionTokenMethodKey>;
