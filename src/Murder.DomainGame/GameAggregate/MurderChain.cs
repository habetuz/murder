using System.Data;

namespace Murder.DomainGame.GameAggregate;

internal class MurderChain
{
    private readonly LinkedList<PlayerStateMapping> _chain;

    internal MurderChain(PlayerId[] participants, IShuffleParticipants participantsShuffler)
    {
        participantsShuffler.Shuffle(participants);

        _chain = new(participants.Select(player => new PlayerStateMapping(player)));
    }

    /// <summary>
    /// Finds the victim of a given murder and returns it.
    /// </summary>
    /// <param name="murder">The murder of whom the victim should be found.</param>
    /// <returns>The victim of the given murder</returns>
    /// <exception cref="NotEnoughParticipantsException"></exception>
    /// <exception cref="PlayerNotParticipating"></exception>
    /// <exception cref="PlayerDeadException"></exception>
    /// <exception cref="NoMoreVictimsException"></exception>
    internal PlayerId Victim(PlayerId murder)
    {
        if (_chain.Count <= 1)
        {
            throw new NotEnoughParticipantsException(_chain.Count);
        }

        return FindNextAlive(murder);
    }

    /// <summary>
    /// Kills the provided victim, if the correct victim of the provided murder.
    /// Returns null if there are no more victims left.
    /// </summary>
    /// <param name="murder">The murder killing the victim</param>
    /// <param name="victim">The victim being killed by the murder</param>
    /// <returns>The next victim of the murder</returns>
    /// <exception cref="IncorrectVictimException"></exception>
    /// <exception cref="PlayerNotParticipating"></exception>
    /// <exception cref="PlayerDeadException"></exception>
    /// <exception cref="NoMoreVictimsException"></exception>
    internal PlayerId? Kill(PlayerId murder, PlayerId victim)
    {
        var correctVictim = FindNextAlive(murder);
        if (correctVictim != victim)
        {
            throw new IncorrectVictimException(murder, correctVictim, victim);
        }

        var victimNode = _chain.Find(new(victim))!;
        victimNode.ValueRef.State = PlayerState.Dead;
        var murderNode = _chain.Find(new(murder))!;
        murderNode.ValueRef.Kills++;
        if (VictimsAvailable())
        {
            return FindNextAlive(murder);
        }
        else
        {
            return null;
        }
    }

    internal bool VictimsAvailable()
    {
        var alivePlayers = _chain.Sum(node => node.State is PlayerState.Alive ? 1 : 0);
        return alivePlayers >= 2;
    }

    internal Dictionary<PlayerId, uint> Leaderboard()
    {
        return new(
            _chain.Select(node => new KeyValuePair<PlayerId, uint>(node.Player, node.Kills))
        );
    }

    internal PlayerId[] Participants()
    {
        return [.. _chain.Select(node => node.Player)];
    }

    /// <summary>
    /// Finds the next alive player in the chain after the specified murderer.
    /// </summary>
    /// <param name="murder">The <see cref="PlayerId"/> of the player who is looking for a victim.</param>
    /// <returns>
    /// The <see cref="PlayerId"/> of the next alive player in the chain.
    /// </returns>
    /// <exception cref="PlayerNotParticipating"></exception>
    /// <exception cref="PlayerDeadException"></exception>
    /// <exception cref="NoMoreVictimsException"></exception>
    private PlayerId FindNextAlive(PlayerId murder)
    {
        var murderNode = _chain.Find(new(murder)) ?? throw new PlayerNotParticipating(murder);
        if (murderNode.Value.State is PlayerState.Dead)
        {
            throw new PlayerDeadException(murder);
        }

        LinkedListNode<PlayerStateMapping>? victim = murderNode;

        do
        {
            victim = victim.Next;
            victim ??= _chain.First!; // Chain has more than 1 value here
        } while (victim.Value.State is PlayerState.Dead && victim.Value.Player != murder);

        if (victim.Value.Player == murder)
        {
            throw new NoMoreVictimsException();
        }

        return victim.Value.Player;
    }

    private struct PlayerStateMapping(PlayerId player, PlayerState state = PlayerState.Alive)
        : IEquatable<PlayerStateMapping>
    {
        internal PlayerId Player { get; } = player;
        internal PlayerState State { get; set; } = state;
        internal uint Kills { get; set; } = 0;

        public readonly bool Equals(PlayerStateMapping other) => Player == other.Player;

        public override readonly bool Equals(object? obj) =>
            obj is PlayerStateMapping other && Equals(other);

        public override readonly int GetHashCode() => Player.GetHashCode();
    }
}
