namespace Murder.DomainGame.GameAggregate;

public sealed class UnexpectedGameStateException(GameState expected, GameState actual)
    : Exception(
        $"Function only available in game state '{expected}', but current state is '{actual}'."
    )
{
    public GameState Expected { get; } = expected;
    public GameState Actual { get; } = actual;
}
