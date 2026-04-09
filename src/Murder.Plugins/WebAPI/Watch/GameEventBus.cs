using System.Threading.Channels;
using Murder.DomainGame;

namespace Murder.Plugins.WebAPI.Watch;

public enum GameEvent
{
    Updated,
    Deleted,
}

public sealed class GameEventBus
{
    private readonly Lock _lock = new();
    private readonly Dictionary<GameId, HashSet<Channel<GameEvent>>> _subscribers = [];

    public void Notify(GameId gameId)
    {
        Publish(gameId, GameEvent.Updated);
    }

    public void NotifyDeleted(GameId gameId)
    {
        Publish(gameId, GameEvent.Deleted);
    }

    public Channel<GameEvent> Subscribe(GameId gameId)
    {
        var channel = Channel.CreateBounded<GameEvent>(new BoundedChannelOptions(16)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false,
        });

        lock (_lock)
        {
            if (!_subscribers.TryGetValue(gameId, out var set))
            {
                set = [];
                _subscribers[gameId] = set;
            }

            set.Add(channel);
        }

        return channel;
    }

    public void Unsubscribe(GameId gameId, Channel<GameEvent> channel)
    {
        channel.Writer.TryComplete();

        lock (_lock)
        {
            if (_subscribers.TryGetValue(gameId, out var set))
            {
                set.Remove(channel);
                if (set.Count == 0)
                {
                    _subscribers.Remove(gameId);
                }
            }
        }
    }

    private void Publish(GameId gameId, GameEvent evt)
    {
        lock (_lock)
        {
            if (!_subscribers.TryGetValue(gameId, out var set))
            {
                return;
            }

            foreach (var channel in set)
            {
                channel.Writer.TryWrite(evt);
            }
        }
    }
}
