using Murder.DomainGame.GameAggregate;
using Murder.DomainGame.Tests.Fakes;

namespace Murder.DomainGame.Tests.GameAggregate;

public class GameTests
{
    private static readonly GameId TestGameId = new("game-1");
    private static readonly PlayerId AdminId = new("player-admin");

    // ── State tests ──────────────────────────────────────────────────────────

    [Fact]
    public void NewGame_IsInPendingState()
    {
        var clock = new FakeDateTimeOffsetProvider();
        var game = new Game(TestGameId, "Night of Knives", AdminId, clock);

        Assert.Equal(GameState.Pending, game.State);
    }

    [Fact]
    public void Game_IsInPendingState_WhenStartTimeIsInTheFuture()
    {
        var now = DateTimeOffset.UtcNow;
        var clock = new FakeDateTimeOffsetProvider { Now = now };
        var game = new Game(TestGameId, "Night of Knives", AdminId, clock);

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
        var game = new Game(TestGameId, "Night of Knives", AdminId, clock);

        game.Start();

        Assert.Equal(GameState.Running, game.State);
    }

    [Fact]
    public void Game_IsInRunningState_WhenEndTimeIsInTheFuture()
    {
        var now = DateTimeOffset.UtcNow;
        var clock = new FakeDateTimeOffsetProvider { Now = now };
        var game = new Game(TestGameId, "Night of Knives", AdminId, clock);

        game.Start();
        game.EndTime = now.AddHours(1);

        Assert.Equal(GameState.Running, game.State);
    }

    [Fact]
    public void Game_IsInEndedState_WhenEndTimeHasPassed()
    {
        var now = DateTimeOffset.UtcNow;
        var clock = new FakeDateTimeOffsetProvider { Now = now };
        var game = new Game(TestGameId, "Night of Knives", AdminId, clock);

        game.Start();
        game.EndTime = now.AddHours(-1); // end time is in the past

        Assert.Equal(GameState.Ended, game.State);
    }
}
