namespace Murder.DomainIdentity;

public interface ICredentialRepository
{
    public CredentialId Store<TMethod>(IdentityId identity, IStoredCredential<TMethod> credential)
        where TMethod : IAuthenticationMethodKey;
    public void Update<TMethod>(CredentialId id, IStoredCredential<TMethod> credential) where TMethod : IAuthenticationMethodKey;
    public (IdentityId identity, IStoredCredential<TMethod> credential)? FindById<TMethod>(
        CredentialId credentialId
    )
        where TMethod : IAuthenticationMethodKey;
    public CredentialId[] FindAll<TMethod>(IdentityId identity)
        where TMethod : IAuthenticationMethodKey;
    public CredentialId[] FindAll<TMethod>() where TMethod : IAuthenticationMethodKey;
    public void Delete(CredentialId credential);
    public void DeleteAll<TMethod>(IdentityId identity)
        where TMethod : IAuthenticationMethodKey;
}
