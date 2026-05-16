using Murder.DomainIdentity;

namespace Murder.Adapters.AuthenticationMethod.Password;

public record struct PasswordEnrollmentData(string Password)
    : IEnrollmentData<PasswordMethodKey>;
