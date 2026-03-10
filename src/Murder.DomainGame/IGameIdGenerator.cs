namespace Murder.DomainGame;

public interface IGameIdGenerator
{
    public GameId GenerateUnique();
}
