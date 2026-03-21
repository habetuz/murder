namespace Murder.DomainIdentity;

public class User(IdentityId id, string name) : Identity(id, name)
{
    public UserState State { get; private set; } = UserState.Active;

    public void Deactivate()
    {
        State = UserState.Deactivated;
    }
}
