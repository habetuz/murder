namespace Murder.DomainIdentity;

public interface IIdentityRepository
{
    public IdentityFactory IdentityFactory { get; }
    public Identity IdentityById(IdentityId id);
    public IdentityId IdentityOfName(string name);
    public void Store(Identity identity);
    public void Update(Identity identity);
}
