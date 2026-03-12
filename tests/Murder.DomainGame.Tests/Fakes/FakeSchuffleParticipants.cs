namespace Murder.DomainGame.Tests.Fakes;

internal class FakeShuffleParticipants : IShuffleParticipants
{
    void IShuffleParticipants.Shuffle(PlayerId[] participants)
    {
        // Do not shuffle
    }
}