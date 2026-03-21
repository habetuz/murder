using Murder.DomainIdentity;

namespace Murder.ApplicationIdentity;

public class AuthenticationService(Authenticator authenticator, ICredentialRepository credentialRepository)
{
    private readonly Authenticator _authenticator = authenticator;
    private readonly ICredentialRepository _credentialRepository = credentialRepository;

    public IdentityId? Authenticate<TMethod>(IIncomingCredential<TMethod> credential) where TMethod : IAuthenticationMethodKey
    {
        return _authenticator.Authenticate(credential);
    }

    public void RevokeCredential(CredentialId credential)
    {
        _credentialRepository.Delete(credential);
    }

    public string? AddMethod<TMethod>(IdentityId identity, IEnrollmentData<TMethod> enrollmentData) where TMethod : IAuthenticationMethodKey
    {
        var enrollmentResult = _authenticator.Enroll(enrollmentData);
        if (enrollmentResult.StoredCredential is not null)
        {
            _credentialRepository.Store(identity, enrollmentResult.StoredCredential);
        }

        return enrollmentResult.DisplayData;
    }

    public void RemoveMethod<TMethod>(IdentityId identity) where TMethod : IAuthenticationMethodKey
    {
        _credentialRepository.DeleteAll<TMethod>(identity);
    }

}