namespace Murder.DomainGame.GameAggregate;

public class UnexpectedGameStateException(GameState expected, GameState actual) : Exception
{
    public override string Message =>
        $"Function only available in game state {expected}, but current state is {actual}";
}
