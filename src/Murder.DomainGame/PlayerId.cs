namespace Murder.DomainGame;

public readonly record struct PlayerId(string Id)
{
    public override string ToString()
    {
        return Id;
    }
}
