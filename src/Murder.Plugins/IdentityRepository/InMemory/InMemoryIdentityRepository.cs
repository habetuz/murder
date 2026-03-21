using Murder.DomainIdentity;

namespace Murder.Plugins.IdentityRepository.InMemory;

public class InMemoryIdentityRepository : IIdentityRepository, IIdentityIdGenerator
{
    public IdentityFactory IdentityFactory { get; }

    private readonly Dictionary<IdentityId, Identity> _identities = [];

    public InMemoryIdentityRepository()
    {
        IdentityFactory = new IdentityFactory(this);
    }

    public IdentityId GenerateUnique()
    {
        return new IdentityId(Guid.NewGuid().ToString());
    }

    public Identity IdentityById(IdentityId id)
    {
        return _identities[id];
    }

    public IdentityId IdentityOfName(string name)
    {
        throw new NotImplementedException();
    }

    public void Store(Identity identity)
    {
        if (_identities.ContainsKey(identity.Id))
        {
            throw new ArgumentException("Identity already exists");
        }

        _identities[identity.Id] = identity;
    }

    public void Update(Identity identity)
    {
        if (!_identities.ContainsKey(identity.Id))
        {
            throw new ArgumentException("Identity does not exist");
        }

        _identities[identity.Id] = identity;
    }
}