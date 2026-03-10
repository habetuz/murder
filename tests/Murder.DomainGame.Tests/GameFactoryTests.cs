using Murder.DomainGame.Tests.Fakes;

namespace Murder.DomainGame.Tests;

public class GameFactoryTests
{
    private readonly FakeGameIdGenerator _idGenerator = new();
    private readonly PlayerId _admin = new("player-1");

    [Fact]
    public void CreateGame_ReturnsGame_WithTheSameNameProvided()
    {
        var factory = new GameFactory(_idGenerator);

        var game = factory.CreateGame("The Haunted Mansion", _admin);

        Assert.Equal("The Haunted Mansion", game.Name);
    }

    [Fact]
    public void CreateGame_ReturnsGame_WithTheSameAdminProvided()
    {
        var factory = new GameFactory(_idGenerator);

        var game = factory.CreateGame("The Haunted Mansion", _admin);

        Assert.Equal(_admin, game.Admin);
    }

    [Fact]
    public void CreateGame_ReturnsGame_WithUniqueIdsOnEachCall()
    {
        var factory = new GameFactory(_idGenerator);

        var game1 = factory.CreateGame("Game One", _admin);
        var game2 = factory.CreateGame("Game Two", _admin);

        Assert.NotEqual(game1.Id, game2.Id);
    }
}
