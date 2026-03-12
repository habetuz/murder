using Murder.DomainGame.GameAggregate;
using Murder.DomainGame.Tests.Fakes;

namespace Murder.DomainGame.Tests.GameAggregate;

public class MurderChainTests
{
    private static readonly PlayerId PlayerA = new("player-a");
    private static readonly PlayerId PlayerB = new("player-b");
    private static readonly PlayerId PlayerC = new("player-c");
    private static readonly IShuffleParticipants participantsShuffler = new FakeShuffleParticipants();

    // ── Victim ────────────────────────────────────────────────────────────────

    [Fact]
    public void Victim_ReturnsNextPlayerInChain()
    {
        var chain = new MurderChain([PlayerA, PlayerB, PlayerC], participantsShuffler);

        Assert.Equal(PlayerB, chain.Victim(PlayerA));
    }

    [Fact]
    public void Victim_Throws_WhenSingleParticipant()
    {
        var chain = new MurderChain([PlayerA], participantsShuffler);

        Assert.Throws<NotEnoughParticipantsException>(() => chain.Victim(PlayerA));
    }

    // ── Kill ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Kill_SkipsDeadPlayer_AndReturnsNextAliveVictim()
    {
        // Chain: A → B → C. Killing B means A's new victim is C.
        var chain = new MurderChain([PlayerA, PlayerB, PlayerC], participantsShuffler);

        var nextVictim = chain.Kill(PlayerA, PlayerB);

        Assert.Equal(PlayerC, nextVictim);
    }

    [Fact]
    public void Kill_ReturnsNull_WhenLastVictimIsKilled()
    {
        var chain = new MurderChain([PlayerA, PlayerB], participantsShuffler);

        var nextVictim = chain.Kill(PlayerA, PlayerB);

        Assert.Null(nextVictim);
    }

    [Fact]
    public void Kill_Throws_WhenIncorrectVictimProvided()
    {
        // A's correct victim is B, not C
        var chain = new MurderChain([PlayerA, PlayerB, PlayerC], participantsShuffler);

        Assert.Throws<IncorrectVictimException>(() => chain.Kill(PlayerA, PlayerC));
    }

    // ── Leaderboard ───────────────────────────────────────────────────────────

    [Fact]
    public void Leaderboard_TracksKillsPerPlayer()
    {
        var chain = new MurderChain([PlayerA, PlayerB, PlayerC], participantsShuffler);
        chain.Kill(PlayerA, PlayerB); // A kills B; A's next victim is C
        chain.Kill(PlayerA, PlayerC); // A kills C

        var leaderboard = chain.Leaderboard();

        Assert.Equal(2u, leaderboard[PlayerA]);
        Assert.Equal(0u, leaderboard[PlayerB]);
    }
}
