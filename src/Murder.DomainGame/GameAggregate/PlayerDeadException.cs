namespace Murder.DomainGame.GameAggregate;

public class PlayerDeadException(PlayerId player) : Exception
{
    public override string Message => $"Action cannot be performed by {player} as it is dead.";
}
