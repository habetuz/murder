namespace Murder.DomainGame.GameAggregate;

public sealed class PlayerDeadException(PlayerId player)
    : Exception($"Action cannot be performed by '{player}' as they are dead.")
{
    public PlayerId Player { get; } = player;
}
