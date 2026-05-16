using Murder.DomainIdentity;

namespace Murder.Adapters.AuthenticationMethod.Password;

public record struct PasswordIncomingCredential(IdentityId IdentityId, string Password)
    : IIncomingCredential<PasswordMethodKey>;
