namespace Murder.DomainGame.Tests.Fakes;

internal class FakeDateTimeOffsetProvider : TimeProvider
{
    public DateTimeOffset Now { get; set; } = DateTimeOffset.UtcNow;

    public override DateTimeOffset GetUtcNow() => Now.ToUniversalTime();
}
