namespace Murder.DomainIdentity;

public abstract class Identity(IdentityId id, string name)
{
    public IdentityId Id { get; } = id;
    public string Name { get; } = name;
}