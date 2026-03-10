using Murder.DomainGame;

namespace Murder.DomainGame.Tests.Fakes;

internal class FakeDateTimeOffsetProvider : IDateTimeOffsetProvider
{
    public DateTimeOffset Now { get; set; } = DateTimeOffset.UtcNow;
}
