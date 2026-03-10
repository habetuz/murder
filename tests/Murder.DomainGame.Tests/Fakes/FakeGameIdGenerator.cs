using Murder.DomainGame;

namespace Murder.DomainGame.Tests.Fakes;

internal class FakeGameIdGenerator : IGameIdGenerator
{
    private int _counter = 0;

    public GameId GenerateUnique() => new($"{++_counter}");
}
