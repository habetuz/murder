namespace Murder.DomainGame.GameAggregate;

public class IncorrectVictimException(
    PlayerId murder,
    PlayerId correctVictim,
    PlayerId incorrectVictim
) : Exception
{
    public override string Message =>
        $"Player {murder} cannot kill {incorrectVictim} as {correctVictim} is the current victim";
}
