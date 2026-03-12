using Murder.DomainGame;

namespace Murder.ApplicationGame;

public class GameNotFoundException(GameId game) : Exception
{
    public override string Message => $"Game {game.Id} does not exist";
}
