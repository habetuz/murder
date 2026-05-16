namespace Murder.Adapters.WebAPI.Authentication;

public static class AuthenticationSettings
{
    public static readonly TimeSpan SessionTokenLifetime = TimeSpan.FromHours(24);
}
