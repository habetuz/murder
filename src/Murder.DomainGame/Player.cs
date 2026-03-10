namespace Murder.DomainGame;

public class Player(PlayerId id, GameId[] games)
{
    public PlayerId Id { get; } = id;
    public GameId[] Games { get; set; } = games;
}
