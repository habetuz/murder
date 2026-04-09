namespace Murder.DomainIdentity;

public abstract class Identity(IdentityId id)
{
    public IdentityId Id { get; } = id;
}
