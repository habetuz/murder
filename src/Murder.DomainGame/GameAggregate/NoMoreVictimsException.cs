namespace Murder.DomainGame.GameAggregate;

public sealed class NoMoreVictimsException()
    : Exception("There are no more available victims in this game.");
