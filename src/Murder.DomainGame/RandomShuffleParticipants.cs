namespace Murder.DomainGame;

internal class RandomShuffleParticipants : IShuffleParticipants
{
    void IShuffleParticipants.Shuffle(PlayerId[] participants)
    {
        Random.Shared.Shuffle(participants);
    }
}