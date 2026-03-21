using Murder.DomainGame;
using Murder.DomainGame.GameAggregate;

namespace Murder.Plugins.GameRepository.InMemory;

public class InMemoryGameRepository(
    uint idLength = 5,
    string idCharset = "0123456789ABCDEFGHJKLMNPQRSTUVWXYZ"
) : IGameRepository, IGameIdGenerator
{
    public GameFactory GameFactory => new(this);

    private readonly Dictionary<GameId, Game> _games = [];
    private readonly uint _idLength = idLength;
    private readonly string _idCharset = idCharset;

    public void Delete(GameId game)
    {
        _games.Remove(game);
    }

    public Game? FindGameById(GameId gameId)
    {
        _games.TryGetValue(gameId, out Game? game);
        return game;
    }

    public Player? FindPlayerById(PlayerId playerId)
    {
        var participatingGames =
            from pair in _games
            where pair.Value.Participants.Contains(playerId)
            select pair.Key;

        if (!participatingGames.Any())
        {
            return null;
        }

        return new Player(playerId, [.. participatingGames]);
    }

    public GameId GenerateUnique()
    {
        GameId gameId;
        do
        {
            var id = "";
            foreach (int i in Enumerable.Range(0, (int)_idLength))
            {
                id += _idCharset[Random.Shared.Next(_idCharset.Length)];
            }

            gameId = new GameId(id);
        } while (_games.ContainsKey(gameId));

        return gameId;
    }

    public GameId[] ListPublic()
    {
        return
        [
            .. from pair in _games where pair.Value.Visibility is Visibility.Public select pair.Key,
        ];
    }

    public void Store(Game game)
    {
        if (_games.ContainsKey(game.Id))
        {
            throw new ArgumentException($"Game with ID {game.Id} already exists");
        }

        _games[game.Id] = game;
    }

    public void Update(Game game)
    {
        if (!_games.ContainsKey(game.Id))
        {
            throw new ArgumentException($"Game with ID {game.Id} does not exist");
        }

        _games[game.Id] = game;
    }
}
