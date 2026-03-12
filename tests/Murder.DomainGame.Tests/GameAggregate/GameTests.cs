using Murder.DomainGame.GameAggregate;
using Murder.DomainGame.Tests.Fakes;

namespace Murder.DomainGame.Tests.GameAggregate;

public class GameTests
{
    private static readonly GameId TestGameId = new("game-1");
    private static readonly PlayerId AdminId = new("player-admin");
    private static readonly IShuffleParticipants participantsShuffler =
        new FakeShuffleParticipants();

    // ── State tests ──────────────────────────────────────────────────────────

    [Fact]
    public void NewGame_IsInPendingState()
    {
        var clock = new FakeDateTimeOffsetProvider();
        var game = new Game(TestGameId, "Night of Knives", AdminId, clock, participantsShuffler);

        Assert.Equal(GameState.Pending, game.State);
    }

    [Fact]
    public void Game_IsInPendingState_WhenStartTimeIsInTheFuture()
    {
        var now = DateTimeOffset.UtcNow;
        var clock = new FakeDateTimeOffsetProvider { Now = now };
        var game = new Game(TestGameId, "Night of Knives", AdminId, clock, participantsShuffler);

        // Start at a moment in the past so StartTime is recorded, then move
        // the clock back before that moment.
        clock.Now = now;
        game.Start(); // StartTime = now
        clock.Now = now.AddSeconds(-1); // "now" is before StartTime

        Assert.Equal(GameState.Pending, game.State);
    }

    [Fact]
    public void Game_IsInRunningState_AfterStart()
    {
        var now = DateTimeOffset.UtcNow;
        var clock = new FakeDateTimeOffsetProvider { Now = now };
        var game = new Game(TestGameId, "Night of Knives", AdminId, clock, participantsShuffler);

        game.Start();

        Assert.Equal(GameState.Running, game.State);
    }

    [Fact]
    public void Game_IsInRunningState_WhenEndTimeIsInTheFuture()
    {
        var now = DateTimeOffset.UtcNow;
        var clock = new FakeDateTimeOffsetProvider { Now = now };
        var game = new Game(TestGameId, "Night of Knives", AdminId, clock, participantsShuffler);

        game.Start();
        game.EndTime = now.AddHours(1);

        Assert.Equal(GameState.Running, game.State);
    }

    [Fact]
    public void Game_IsInEndedState_WhenEndTimeHasPassed()
    {
        var now = DateTimeOffset.UtcNow;
        var clock = new FakeDateTimeOffsetProvider { Now = now };
        var game = new Game(TestGameId, "Night of Knives", AdminId, clock, participantsShuffler);

        game.Start();
        game.EndTime = now.AddHours(-1); // end time is in the past

        Assert.Equal(GameState.Ended, game.State);
    }

    // ── Join / Remove ─────────────────────────────────────────────────────────

    [Fact]
    public void Join_AddsParticipant_ToPendingGame()
    {
        var clock = new FakeDateTimeOffsetProvider();
        var game = new Game(TestGameId, "Night of Knives", AdminId, clock, participantsShuffler);

        game.Join(PlayerB);

        Assert.Contains(PlayerB, game.Participants);
    }

    [Fact]
    public void Remove_PromotesFirstRemainingPlayer_AsAdmin_WhenAdminLeaves()
    {
        var clock = new FakeDateTimeOffsetProvider();
        var game = new Game(TestGameId, "Night of Knives", AdminId, clock, participantsShuffler);
        game.Join(PlayerB);

        game.Remove(AdminId);

        Assert.Equal(PlayerB, game.Admin);
    }

    // ── Victim (mirrors MurderChainTests.Victim_ReturnsNextPlayerInChain) ─────

    [Fact]
    public void Victim_ReturnsNextParticipant_InJoinOrder()
    {
        var clock = new FakeDateTimeOffsetProvider();
        var game = CreateRunningGame(clock, AdminId, PlayerB, PlayerC);

        Assert.Equal(PlayerB, game.Victim(AdminId));
    }

    // ── Kill (mirrors MurderChainTests.Kill_SkipsDeadPlayer_AndReturnsNextAliveVictim) ──

    [Fact]
    public void Kill_ReturnsNextVictim_AfterKill()
    {
        var clock = new FakeDateTimeOffsetProvider();
        var game = CreateRunningGame(clock, AdminId, PlayerB, PlayerC);

        var nextVictim = game.Kill(AdminId, PlayerB);

        Assert.Equal(PlayerC, nextVictim);
    }

    // ── Leaderboard (mirrors MurderChainTests.Leaderboard_TracksKillsPerPlayer) ──

    [Fact]
    public void Leaderboard_ReflectsKillCounts_AfterKills()
    {
        var clock = new FakeDateTimeOffsetProvider();
        var game = CreateRunningGame(clock, AdminId, PlayerB, PlayerC);
        game.Kill(AdminId, PlayerB); // AdminId kills PlayerB; next victim is PlayerC
        game.Kill(AdminId, PlayerC); // AdminId kills PlayerC

        var leaderboard = game.Leaderboard();

        Assert.Equal(2u, leaderboard[AdminId]);
        Assert.Equal(0u, leaderboard[PlayerB]);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static readonly PlayerId PlayerB = new("player-b");
    private static readonly PlayerId PlayerC = new("player-c");

    private static Game CreateRunningGame(
        FakeDateTimeOffsetProvider clock,
        PlayerId admin,
        params PlayerId[] additionalPlayers
    )
    {
        var game = new Game(TestGameId, "Night of Knives", admin, clock, participantsShuffler);
        foreach (var player in additionalPlayers)
            game.Join(player);
        game.Start();
        return game;
    }
}
