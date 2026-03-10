namespace Murder.DomainGame;

internal interface IDateTimeOffsetProvider
{
    DateTimeOffset Now { get; }
}
