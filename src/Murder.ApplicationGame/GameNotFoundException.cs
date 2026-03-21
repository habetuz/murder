using Murder.DomainGame;

namespace Murder.ApplicationGame;

public sealed class GameNotFoundException(GameId game)
    : Exception($"Game '{game.Id}' does not exist.")
{
    public GameId Game { get; } = game;
}
