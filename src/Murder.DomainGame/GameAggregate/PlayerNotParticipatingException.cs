namespace Murder.DomainGame.GameAggregate;

public class PlayerNotParticipating(PlayerId player) : Exception
{
    public override string Message => $"Player {player} is not participating at this game";
}
