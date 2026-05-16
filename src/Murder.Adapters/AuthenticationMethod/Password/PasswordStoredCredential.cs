using Murder.DomainIdentity;

namespace Murder.Adapters.AuthenticationMethod.Password;

public record struct PasswordStoredCredential(string SaltBase64, string HashBase64, int Iterations)
    : IStoredCredential<PasswordMethodKey>;
