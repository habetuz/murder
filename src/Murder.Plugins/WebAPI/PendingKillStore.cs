using Murder.DomainGame;

namespace Murder.Plugins.WebAPI;

public sealed record PendingKill(PlayerId Killer, PlayerId Victim);

public sealed class PendingKillStore
{
    private readonly Lock _lock = new();

    // One pending kill per game at a time (a killer can only have one outstanding claim)
    // Key: (gameId, killerId)
    private readonly Dictionary<(GameId, PlayerId), PendingKill> _pendingByKiller = [];

    // Reverse index: (gameId, victimId) → killerId, for quick victim lookup
    private readonly Dictionary<(GameId, PlayerId), PlayerId> _pendingByVictim = [];

    public void Add(GameId gameId, PlayerId killer, PlayerId victim)
    {
        lock (_lock)
        {
            // Remove any existing pending kill by this killer in this game
            Remove(gameId, killer);

            var pending = new PendingKill(killer, victim);
            _pendingByKiller[(gameId, killer)] = pending;
            _pendingByVictim[(gameId, victim)] = killer;
        }
    }

    public PendingKill? GetForKiller(GameId gameId, PlayerId killer)
    {
        lock (_lock)
        {
            return _pendingByKiller.GetValueOrDefault((gameId, killer));
        }
    }

    public bool HasPendingKillAsVictim(GameId gameId, PlayerId victim)
    {
        lock (_lock)
        {
            return _pendingByVictim.ContainsKey((gameId, victim));
        }
    }

    /// <summary>
    /// Removes the pending kill where the given player is the victim.
    /// Returns the PendingKill if one existed, null otherwise.
    /// </summary>
    public PendingKill? RemoveForVictim(GameId gameId, PlayerId victim)
    {
        lock (_lock)
        {
            if (!_pendingByVictim.Remove((gameId, victim), out var killer))
            {
                return null;
            }

            _pendingByKiller.Remove((gameId, killer));
            return new PendingKill(killer, victim);
        }
    }

    /// <summary>
    /// Removes any pending kill by the given killer in the given game.
    /// </summary>
    public void Remove(GameId gameId, PlayerId killer)
    {
        lock (_lock)
        {
            if (_pendingByKiller.Remove((gameId, killer), out var pending))
            {
                _pendingByVictim.Remove((gameId, pending.Victim));
            }
        }
    }

    /// <summary>
    /// Removes all pending kills for a game (e.g. when game ends/is deleted).
    /// </summary>
    public void RemoveAll(GameId gameId)
    {
        lock (_lock)
        {
            var killersToRemove = _pendingByKiller
                .Where(kv => kv.Key.Item1 == gameId)
                .Select(kv => kv.Key)
                .ToList();

            foreach (var key in killersToRemove)
            {
                if (_pendingByKiller.Remove(key, out var pending))
                {
                    _pendingByVictim.Remove((gameId, pending.Victim));
                }
            }
        }
    }
}
