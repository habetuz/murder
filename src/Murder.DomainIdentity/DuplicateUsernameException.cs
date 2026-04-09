namespace Murder.DomainIdentity;

public sealed class DuplicateUsernameException(string name)
    : Exception($"A user with the name '{name}' already exists.")
{
    public string Name { get; } = name;
}
