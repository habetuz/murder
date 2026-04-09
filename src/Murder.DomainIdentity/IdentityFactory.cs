namespace Murder.DomainIdentity;

public class IdentityFactory(IIdentityIdGenerator idGenerator)
{
    private readonly IIdentityIdGenerator _idGenerator = idGenerator;

    public User CreateUser(string name)
    {
        return new User(_idGenerator.GenerateUnique(), name);
    }

    public Guest CreateGuest()
    {
        return new Guest(_idGenerator.GenerateUnique());
    }
}
