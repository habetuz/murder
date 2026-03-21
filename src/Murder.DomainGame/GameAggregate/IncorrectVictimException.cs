namespace Murder.DomainGame.GameAggregate;

public sealed class IncorrectVictimException(
    PlayerId murder,
    PlayerId correctVictim,
    PlayerId incorrectVictim
)
    : Exception(
        $"Player '{murder}' cannot kill '{incorrectVictim}' as '{correctVictim}' is the current victim."
    )
{
    public PlayerId Murder { get; } = murder;
    public PlayerId CorrectVictim { get; } = correctVictim;
    public PlayerId IncorrectVictim { get; } = incorrectVictim;
}
