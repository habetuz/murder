namespace Murder.DomainGame.GameAggregate;

public sealed class NotEnoughParticipantsException(int participants)
    : Exception(
        $"Game does not have enough players. At least 2 are needed but only {participants} are participating."
    )
{
    public int Participants { get; } = participants;
}
