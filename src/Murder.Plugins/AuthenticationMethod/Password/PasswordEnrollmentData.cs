using Murder.DomainIdentity;

namespace Murder.Plugins.AuthenticationMethod.Password;

public record struct PasswordEnrollmentData(string Password)
    : IEnrollmentData<PasswordMethodKey>;
