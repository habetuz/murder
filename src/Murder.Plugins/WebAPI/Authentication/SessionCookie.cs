namespace Murder.Plugins.WebAPI.Authentication;

public static class SessionCookie
{
    public static void Append(HttpResponse response, HttpRequest request, string sessionToken)
    {
        response.Cookies.Append(
            AuthenticationCookieNames.SessionToken,
            sessionToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.Add(AuthenticationSettings.SessionTokenLifetime),
                IsEssential = true,
                Path = "/",
            }
        );
    }

    public static void Expire(HttpResponse response, HttpRequest request)
    {
        response.Cookies.Append(
            AuthenticationCookieNames.SessionToken,
            string.Empty,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UnixEpoch,
                IsEssential = true,
                Path = "/",
            }
        );
    }
}