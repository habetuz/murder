using Murder.DomainIdentity;

namespace Murder.Plugins.AuthenticationMethod.Password;

public record struct PasswordStoredCredential(string SaltBase64, string HashBase64, int Iterations)
    : IStoredCredential<PasswordMethodKey>;
