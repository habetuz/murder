namespace Murder.Plugins.WebAPI.Authentication;

public static class AuthenticationSettings
{
    public static readonly TimeSpan SessionTokenLifetime = TimeSpan.FromMinutes(30);
}
