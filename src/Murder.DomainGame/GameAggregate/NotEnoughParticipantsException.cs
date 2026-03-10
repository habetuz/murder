namespace Murder.DomainGame.GameAggregate;

public class NotEnoughParticipantsException(int participants) : Exception
{
    public override string Message =>
        $"Game does not have enough players. At least 2 are needed but only ${participants} are participating";
}
