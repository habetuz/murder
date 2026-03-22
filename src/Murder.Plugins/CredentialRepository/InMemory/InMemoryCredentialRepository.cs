using Murder.DomainIdentity;

namespace Murder.Plugins.CredentialRepository.InMemory;

public class InMemoryCredentialRepository : ICredentialRepository
{
    private readonly Dictionary<CredentialId, StoredCredentialRecord> _credentials = [];

    public void Delete(CredentialId credential)
    {
        _credentials.Remove(credential);
    }

    public void DeleteAll<TMethod>(IdentityId identity)
        where TMethod : IAuthenticationMethodKey
    {
        var methodType = typeof(TMethod);

        var ids = _credentials
            .Where(pair => pair.Value.Identity == identity && pair.Value.MethodType == methodType)
            .Select(pair => pair.Key)
            .ToArray();

        foreach (var id in ids)
            _credentials.Remove(id);
    }

    public CredentialId[] FindAll<TMethod>(IdentityId identity)
        where TMethod : IAuthenticationMethodKey
    {
        var methodType = typeof(TMethod);

        return
        [
            .. _credentials
                .Where(pair =>
                    pair.Value.Identity == identity && pair.Value.MethodType == methodType
                )
                .Select(pair => pair.Key),
        ];
    }

    public CredentialId[] FindAll<TMethod>() where TMethod : IAuthenticationMethodKey
    {
        var methodType = typeof(TMethod);

        return
        [
            .. _credentials
                .Where(pair =>
                    pair.Value.MethodType == methodType
                )
                .Select(pair => pair.Key),
        ];
    }

    public (IdentityId identity, IStoredCredential<TMethod> credential)? FindById<TMethod>(
          CredentialId credentialId
      )
          where TMethod : IAuthenticationMethodKey
    {
        if (!_credentials.TryGetValue(credentialId, out var stored))
            return null;

        if (stored.MethodType != typeof(TMethod))
            return null;

        if (stored.Credential is not IStoredCredential<TMethod> typedCredential)
            return null;

        return (stored.Identity, typedCredential);
    }

    public CredentialId Store<TMethod>(IdentityId identity, IStoredCredential<TMethod> credential)
        where TMethod : IAuthenticationMethodKey
    {
        CredentialId credentialId;

        do
        {
            credentialId = new CredentialId(Guid.NewGuid().ToString());
        } while (_credentials.ContainsKey(credentialId));

        _credentials[credentialId] = new StoredCredentialRecord(
            typeof(TMethod),
            identity,
            credential
        );
        return credentialId;
    }

    public void Update<TMethod>(CredentialId id, IStoredCredential<TMethod> credential) where TMethod : IAuthenticationMethodKey
    {
        if (!_credentials.TryGetValue(id, out var storedCredential))
        {
            throw new ArgumentException("Credential does not exist");
        }

        _credentials[id] = new StoredCredentialRecord(
            typeof(TMethod),
            storedCredential.Identity,
            credential
        );
    }

    private sealed record StoredCredentialRecord(
          Type MethodType,
          IdentityId Identity,
          object Credential
      );
}
