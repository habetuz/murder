using Murder.DomainGame.GameAggregate;

namespace Murder.DomainGame;

public class GameFactory(IGameIdGenerator gameIdGenerator)
{
    readonly IGameIdGenerator _gameIdGenerator = gameIdGenerator;
    readonly IDateTimeOffsetProvider _dateTimeOffsetProvider = new SystemDateTimeOffsetProvider();
    readonly IShuffleParticipants _participantsShuffler = new RandomShuffleParticipants();

    public Game CreateGame(string name, PlayerId admin, string adminDisplayName, Visibility visibility = Visibility.Private)
    {
        GameId id = _gameIdGenerator.GenerateUnique();
        return new Game(id, name, admin, adminDisplayName, _dateTimeOffsetProvider, _participantsShuffler)
        {
            Visibility = visibility,
        };
    }
}
