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
        var matches = _identities
            .Where(pair => string.Equals(pair.Value.Name, name, StringComparison.OrdinalIgnoreCase))
            .Select(pair => pair.Key)
            .Take(2)
            .ToArray();

        if (matches.Length == 0)
        {
            throw new KeyNotFoundException($"No identity found with name '{name}'.");
        }

        if (matches.Length > 1)
        {
            throw new InvalidOperationException($"Multiple identities found with name '{name}'.");
        }

        return matches[0];
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
