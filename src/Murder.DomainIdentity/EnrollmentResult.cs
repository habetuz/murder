namespace Murder.DomainIdentity;

public readonly record struct EnrollmentResult<TMethod>(
    IStoredCredential<TMethod>? StoredCredential,
    string? DisplayData
)
    where TMethod : IAuthenticationMethodKey;
