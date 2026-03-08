namespace Murder.DomainGame.GameAggregate;

internal class MurderChain(LinkedList<PlayerId> chain)
{
    private LinkedList<PlayerId> _chain = chain;


    internal PlayerId Victim(PlayerId murder)
    {
        throw new NotImplementedException();
    }

    internal PlayerId Kill(PlayerId murder, PlayerId victim)
    {
        throw new NotImplementedException();
    }

    internal Dictionary<PlayerId, uint> Leaderboard()
    {
        throw new NotImplementedException();
    }

    internal PlayerId[] Participants()
    {
        throw new NotImplementedException();
    }
}