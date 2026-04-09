namespace Murder.DomainGame.GameAggregate;

public class Game : IReadOnlyGame
{
    public GameId Id { get; }
    public string Name { get; set; }
    public Visibility Visibility { get; set; } = Visibility.Private;
    public string? Description { get; set; }
    public DateTimeOffset? StartTime { get; private set; }
    public DateTimeOffset? EndTime { get; private set; }
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

    public PlayerId[] Participants
    {
        get =>
            State switch
            {
                GameState.Pending => [.. _tmpParticipants!.Keys],
                GameState.Running or GameState.Ended => _murderChain!.Participants,
                _ => throw new NotImplementedException(),
            };
    }

    public Dictionary<PlayerId, string> ParticipantNames
    {
        get => new(_participantNames);
    }

    MurderChain? _murderChain;
    Dictionary<PlayerId, string>? _tmpParticipants;
    Dictionary<PlayerId, string> _participantNames;
    readonly IDateTimeOffsetProvider _dateTimeProvider;
    readonly IShuffleParticipants _participantShuffler;

    internal Game(
        GameId id,
        string name,
        PlayerId admin,
        string adminDisplayName,
        IDateTimeOffsetProvider dateTimeProvider,
        IShuffleParticipants participantsShuffler
    )
    {
        Id = id;
        Name = name;
        Admin = admin;
        _dateTimeProvider = dateTimeProvider;
        _participantShuffler = participantsShuffler;
        _tmpParticipants = new() { [admin] = adminDisplayName };
        _participantNames = new() { [admin] = adminDisplayName };
    }

    public void Join(PlayerId player, string displayName)
    {
        if (State != GameState.Pending)
        {
            throw new UnexpectedGameStateException(GameState.Pending, State);
        }

        _tmpParticipants![player] = displayName;
        _participantNames[player] = displayName;
        if (_tmpParticipants.Count == 1)
        {
            // This is the first player joining a game, after it had 0 players
            // Make this player the administrator
            Admin = player;
        }
    }

    public void Remove(PlayerId player)
    {
        if (State != GameState.Pending)
        {
            throw new UnexpectedGameStateException(GameState.Pending, State);
        }

        _tmpParticipants!.Remove(player);
        _participantNames.Remove(player);
        if (Admin == player)
        {
            // Admin was removed. Assign a new admin
            // 1. First user in list (longest in game)
            // 2. No user, if game is empty
            Admin = null;
            if (_tmpParticipants.Count > 0)
            {
                Admin = _tmpParticipants.Keys.First();
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

        var newVictim = _murderChain!.Kill(murder, victim);
        if (newVictim is null)
        {
            // If there are new more victims, the game is finished now
            End();
        }

        return newVictim;
    }

    public Dictionary<PlayerId, uint> Leaderboard()
    {
        if (State is GameState.Pending)
        {
            throw new UnexpectedGameStateException(GameState.Running, State);
        }

        return _murderChain!.Leaderboard();
    }

    public void Start()
    {
        if (State is not GameState.Pending)
        {
            throw new UnexpectedGameStateException(GameState.Pending, State);
        }

        if (_tmpParticipants!.Count < 2)
        {
            throw new NotEnoughParticipantsException(_tmpParticipants.Count);
        }

        StartTime = _dateTimeProvider.Now;

        // Unsert end time if it is invalid
        if (EndTime < StartTime)
        {
            EndTime = null;
        }

        _murderChain = new([.. _tmpParticipants!.Keys], _participantShuffler);
        _tmpParticipants = null;
    }

    public void End()
    {
        EndTime = _dateTimeProvider.Now;
    }

    public void SetEnd(DateTimeOffset end)
    {
        if (State is GameState.Ended)
        {
            throw new UnexpectedGameStateException(GameState.Running, GameState.Ended);
        }

        // Clamp end time to not be before start time
        if (end < StartTime)
        {
            EndTime = StartTime;
        }
        else
        {
            EndTime = end;
        }
    }

    public void SetEnd(DateTimeOffset? end)
    {
        throw new NotImplementedException();
    }
}
