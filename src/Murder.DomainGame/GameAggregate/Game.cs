namespace Murder.DomainGame.GameAggregate;

public class Game
{
    public GameId Id { get; }
    public string Name { get; set; }
    string? Description { get; set; }
    public DateTimeOffset? StartTime { get; private set; }
    public DateTimeOffset? EndTime { get; set; }
    public PlayerId Admin { get; }
    public GameState State
    {
        get
        {
            if (StartTime is null || _dateTimeProvider.Now < StartTime)
            {
                return GameState.Pending;
            }

            if (EndTime is not null && EndTime < _dateTimeProvider.Now)
            {
                return GameState.Ended;
            }

            return GameState.Running;
        }
    }
    readonly MurderChain _murderChain = new();
    readonly IDateTimeOffsetProvider _dateTimeProvider;

    internal Game(GameId id, string name, PlayerId admin, IDateTimeOffsetProvider dateTimeProvider)
    {
        Id = id;
        Name = name;
        Admin = admin;
        _dateTimeProvider = dateTimeProvider;
    }

    public PlayerId Victim(PlayerId murder)
    {
        return _murderChain.Victim(murder);
    }

    public PlayerId Kill(PlayerId murder, PlayerId victim)
    {
        return _murderChain.Kill(murder, victim);
    }

    public Dictionary<PlayerId, uint> Leaderboard()
    {
        return _murderChain.Leaderboard();
    }

    public PlayerId[] Participants()
    {
        return _murderChain.Participants();
    }

    public void Start()
    {
        StartTime = _dateTimeProvider.Now;
    }

    public void End()
    {
        StartTime = _dateTimeProvider.Now;
    }
}
