using Murder.DomainGame.GameAggregate;

namespace Murder.DomainGame;

public interface IGameRepository
{
    public Game? FindGameById(GameId gameId);
    public Player? FindPlayerById(PlayerId playerId);
    public GameId[] ListPublic();
    public void Store(Game game);
    public void Update(Game game);
    public void Upsert(Game game);
    public void Delete(Game game);
    public bool ExistsGame(GameId gameId);
    public bool ExistsPlayer(PlayerId playerId);
}
