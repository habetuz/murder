namespace Murder.DomainIdentity;

public sealed class AuthenticationMethodMismatchException(Type expected, Type actual)
    : Exception($"Authentication method mismatch. Expected '{expected.Name}', got '{actual.Name}'.")
{
    public Type Expected { get; } = expected;
    public Type Actual { get; } = actual;
}
