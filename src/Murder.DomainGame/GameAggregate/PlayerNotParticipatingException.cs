namespace Murder.DomainGame.GameAggregate;

public sealed class PlayerNotParticipating(PlayerId player)
    : Exception($"Player '{player}' is not participating in this game.")
{
    public PlayerId Player { get; } = player;
}
