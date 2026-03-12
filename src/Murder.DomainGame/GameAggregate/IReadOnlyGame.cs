namespace Murder.DomainGame.GameAggregate;

public interface IReadOnlyGame
{
    public GameId Id { get; }
    public string Name { get; }
    public Visibility Visibility { get; }
    public string? Description { get; }
    public DateTimeOffset? StartTime { get; }
    public DateTimeOffset? EndTime { get; }
    public PlayerId? Admin { get; }
    public GameState State { get; }
}
