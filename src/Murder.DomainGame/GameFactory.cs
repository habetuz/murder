using Murder.DomainGame.GameAggregate;

namespace Murder.DomainGame;

public class GameFactory(IGameIdGenerator gameIdGenerator)
{
    readonly IGameIdGenerator _gameIdGenerator = gameIdGenerator;
    readonly IDateTimeOffsetProvider _dateTimeOffsetProvider = new SystemDateTimeOffsetProvider();

    public Game CreateGame(string name, PlayerId admin)
    {
        GameId id = _gameIdGenerator.GenerateUnique();
        return new Game(id, name, admin, _dateTimeOffsetProvider);
    }
}
