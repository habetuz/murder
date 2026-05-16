using Murder.DomainGame.GameAggregate;

namespace Murder.DomainGame;

public interface IGameRepository
{
    public GameFactory GameFactory { get; }
    public Game? FindGameById(GameId gameId);
    public Player? FindPlayerById(PlayerId playerId);
    public void Store(Game game);
    public void Update(Game game);
    public void Delete(GameId game);
}
