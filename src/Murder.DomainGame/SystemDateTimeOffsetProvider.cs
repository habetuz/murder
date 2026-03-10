namespace Murder.DomainGame;

internal class SystemDateTimeOffsetProvider : IDateTimeOffsetProvider
{
    public DateTimeOffset Now => DateTimeOffset.Now;
}
