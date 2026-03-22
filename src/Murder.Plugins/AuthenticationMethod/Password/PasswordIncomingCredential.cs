using Murder.DomainIdentity;

namespace Murder.Plugins.AuthenticationMethod.Password;

public record struct PasswordIncomingCredential(IdentityId IdentityId, string Password)
    : IIncomingCredential<PasswordMethodKey>;
