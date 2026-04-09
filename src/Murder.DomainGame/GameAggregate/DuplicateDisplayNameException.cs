namespace Murder.DomainGame.GameAggregate;

public sealed class DuplicateDisplayNameException(string displayName)
    : Exception($"A participant with the display name '{displayName}' is already in this game.")
{
    public string DisplayName { get; } = displayName;
}
