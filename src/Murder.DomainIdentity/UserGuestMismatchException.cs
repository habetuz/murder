namespace Murder.DomainIdentity;

public class UserGuestMismatchException(bool expectedUser)
    : Exception(
        $"Expected identity of type {(expectedUser ? "User" : "Guest")} but received {(expectedUser ? "Guest" : "User")}"
    )
{
    public bool ExpectedUser { get; } = expectedUser;
}
