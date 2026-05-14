using Murder.DomainGame.GameAggregate;
using Murder.DomainGame.Tests.Fakes;

namespace Murder.DomainGame.Tests.GameAggregate;

public class MurderChainTests
{
    private static readonly PlayerId PlayerA = new("player-a");
    private static readonly PlayerId PlayerB = new("player-b");
    private static readonly PlayerId PlayerC = new("player-c");
    private static readonly IShuffleParticipants participantsShuffler =
        new FakeShuffleParticipants();

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

    // ── Forfeit ──────────────────────────────────────────────────────────────

    [Fact]
    public void Forfeit_MarksPlayerDead_WithoutCreditingKill()
    {
        // Chain: A → B → C. B forfeits. A's new victim is C, nobody gets a kill.
        var chain = new MurderChain([PlayerA, PlayerB, PlayerC], participantsShuffler);

        var victimsRemain = chain.Forfeit(PlayerB);

        Assert.True(victimsRemain);
        Assert.Equal(PlayerC, chain.Victim(PlayerA));
        Assert.Equal(0u, chain.Leaderboard()[PlayerA]);
        Assert.Equal(0u, chain.Leaderboard()[PlayerB]);
    }

    [Fact]
    public void Forfeit_ReturnsFalse_WhenOnlyOnePlayerLeft()
    {
        var chain = new MurderChain([PlayerA, PlayerB], participantsShuffler);

        var victimsRemain = chain.Forfeit(PlayerB);

        Assert.False(victimsRemain);
    }

    [Fact]
    public void Forfeit_Throws_WhenPlayerAlreadyDead()
    {
        var chain = new MurderChain([PlayerA, PlayerB, PlayerC], participantsShuffler);
        chain.Kill(PlayerA, PlayerB);

        Assert.Throws<PlayerDeadException>(() => chain.Forfeit(PlayerB));
    }

    [Fact]
    public void Forfeit_Throws_WhenPlayerNotInChain()
    {
        var chain = new MurderChain([PlayerA, PlayerB], participantsShuffler);
        var unknown = new PlayerId("unknown");

        Assert.Throws<PlayerNotParticipating>(() => chain.Forfeit(unknown));
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
