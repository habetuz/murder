namespace Murder.DomainGame.GameAggregate;

public class Game
{
    public GameId Id { get; }
    public string Name { get; set; }
    public Visibility Visibility { get; set; } = Visibility.Private;
    public string? Description { get; set; }
    public DateTimeOffset? StartTime { get; private set; }
    public DateTimeOffset? EndTime { get; set; }
    public PlayerId? Admin { get; private set; }
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
    MurderChain? _murderChain;
    List<PlayerId>? _tmpParticipants;
    readonly IDateTimeOffsetProvider _dateTimeProvider;
    readonly IShuffleParticipants _participantShuffler;

    internal Game(GameId id, string name, PlayerId admin, IDateTimeOffsetProvider dateTimeProvider, IShuffleParticipants participantsShuffler)
    {
        Id = id;
        Name = name;
        Admin = admin;
        _dateTimeProvider = dateTimeProvider;
        _participantShuffler = participantsShuffler;
        _tmpParticipants = [admin];
    }

    public void Join(PlayerId player)
    {
        if (State != GameState.Pending)
        {
            throw new UnexpectedGameStateException(GameState.Pending, State);
        }

        _tmpParticipants!.Add(player);
        if (_tmpParticipants.Count == 1)
        {
            // This is the first player joining a game, after it had 0 players
            // Make this player the administrator
            Admin = player;
        }

        _tmpParticipants = [.. _tmpParticipants.Distinct()]; // Deduplicate
    }

    public void Remove(PlayerId player)
    {
        if (State != GameState.Pending)
        {
            throw new UnexpectedGameStateException(GameState.Pending, State);
        }

        _tmpParticipants!.Remove(player);
        if (Admin == player)
        {
            // Admin was removed. Assign a new admin
            // 1. First user in list (longest in game)
            // 2. No user, if game is empty
            Admin = null;
            if (_tmpParticipants.Count > 0)
            {
                Admin = _tmpParticipants[0];
            }
        }
    }

    public PlayerId Victim(PlayerId murder)
    {
        if (State != GameState.Running)
        {
            throw new UnexpectedGameStateException(GameState.Running, State);
        }

        return _murderChain!.Victim(murder);
    }

    public PlayerId? Kill(PlayerId murder, PlayerId victim)
    {
        if (State != GameState.Running)
        {
            throw new UnexpectedGameStateException(GameState.Running, State);
        }

        return _murderChain!.Kill(murder, victim);
    }

    public Dictionary<PlayerId, uint> Leaderboard()
    {
        if (State is GameState.Pending)
        {
            throw new UnexpectedGameStateException(GameState.Running, State);
        }

        return _murderChain!.Leaderboard();
    }

    public PlayerId[] Participants()
    {
        return State switch
        {
            GameState.Pending => [.. _tmpParticipants!],
            GameState.Running or GameState.Ended => _murderChain!.Participants(),
            _ => throw new NotImplementedException(),
        };
    }

    public void Start()
    {
        if (State is not GameState.Pending)
        {
            throw new UnexpectedGameStateException(GameState.Pending, State);
        }

        StartTime = _dateTimeProvider.Now;
        _murderChain = new([.. _tmpParticipants!], _participantShuffler);
        _tmpParticipants = null;
    }

    public void End()
    {
        EndTime = _dateTimeProvider.Now;
    }
}
