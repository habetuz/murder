namespace Murder.DomainIdentity;

public interface IIdentityIdGenerator
{
    public IdentityId GenerateUnique();
}
