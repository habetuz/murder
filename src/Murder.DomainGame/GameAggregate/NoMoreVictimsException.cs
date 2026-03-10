namespace Murder.DomainGame.GameAggregate;

public class NoMoreVictimsException : Exception
{
    public override string Message => "There are no more available victims in this game.";
}
