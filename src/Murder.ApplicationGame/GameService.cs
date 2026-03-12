using Murder.DomainGame;
using Murder.DomainGame.GameAggregate;

namespace Murder.ApplicationGame;

public class GameService(IGameRepository gameRepository)
{
    readonly IGameRepository _repository = gameRepository;

    public GameId[]? ListJoinedGames(PlayerId player)
    {
        var playerData = _repository.FindPlayerById(player);
        if (playerData is null)
        {
            return null;
        }
        else
        {
            return playerData.Games;
        }
    }

    public GameId[] ListPublicGames()
    {
        return _repository.ListPublic();
    }

    public IReadOnlyGame? GetGame(GameId game)
    {
        return _repository.FindGameById(game);
    }

    public void JoinGame(GameId game, PlayerId player)
    {
        var gameEntity = _repository.FindGameById(game) ?? throw new GameNotFoundException(game);
        gameEntity.Join(player);
        _repository.Update(gameEntity);
    }

    public void LeaveGame(GameId game, PlayerId player)
    {
        var gameEntity = _repository.FindGameById(game) ?? throw new GameNotFoundException(game);
        gameEntity.Remove(player);
        _repository.Update(gameEntity);
    }

    public void StartGame(GameId game, DateTimeOffset? end)
    {
        var gameEntity = _repository.FindGameById(game) ?? throw new GameNotFoundException(game);
        if (end is not null)
        {
            gameEntity.SetEnd(end.Value);
        }

        gameEntity.Start();
        _repository.Update(gameEntity);
    }

    public void EndGame(GameId game)
    {
        var gameEntity = _repository.FindGameById(game) ?? throw new GameNotFoundException(game);
        gameEntity.End();
        _repository.Update(gameEntity);
    }

    public GameId CreateGame(string name, PlayerId admin, Visibility visibility)
    {
        var gameEntity = _repository.GameFactory.CreateGame(name, admin, visibility);
        _repository.Store(gameEntity);
        return gameEntity.Id;
    }

    public void RenameGame(GameId game, string name)
    {
        var gameEntity = _repository.FindGameById(game) ?? throw new GameNotFoundException(game);
        gameEntity.Name = name;
        _repository.Update(gameEntity);
    }

    public void DescribeGame(GameId game, string description)
    {
        var gameEntity = _repository.FindGameById(game) ?? throw new GameNotFoundException(game);
        gameEntity.Description = description;
        _repository.Update(gameEntity);
    }

    public void ChangeEnd(GameId game, DateTimeOffset end)
    {
        var gameEntity = _repository.FindGameById(game) ?? throw new GameNotFoundException(game);
        gameEntity.SetEnd(end);
        _repository.Update(gameEntity);
    }

    public void DeleteGame(GameId game)
    {
        _repository.Delete(game);
    }

    public PlayerId Victim(GameId game, PlayerId murder)
    {
        var gameEntity = _repository.FindGameById(game) ?? throw new GameNotFoundException(game);
        return gameEntity.Victim(murder);
    }

    public PlayerId? Kill(GameId game, PlayerId murder, PlayerId victim)
    {
        var gameEntity = _repository.FindGameById(game) ?? throw new GameNotFoundException(game);
        var nextVictim = gameEntity.Kill(murder, victim);
        _repository.Update(gameEntity);
        return nextVictim;
    }

    public Dictionary<PlayerId, uint> Leaderboard(GameId game)
    {
        var gameEntity = _repository.FindGameById(game) ?? throw new GameNotFoundException(game);
        return gameEntity.Leaderboard();
    }

    public PlayerId[] Participants(GameId game)
    {
        var gameEntity = _repository.FindGameById(game) ?? throw new GameNotFoundException(game);
        return gameEntity.Participants;
    }
}
