using Murder.DomainIdentity;

namespace Murder.Plugins.WebAPI.Authentication;

public static class AuthenticationContextExtensions
{
    public static bool TryGetAuthenticatedIdentity(HttpContext context, out IdentityId identityId)
    {
        identityId = default;

        if (!context.Items.TryGetValue(AuthenticationHttpContextItems.CurrentIdentityId, out var identityObject))
        {
            return false;
        }

        if (identityObject is not IdentityId authenticatedIdentityId)
        {
            return false;
        }

        identityId = authenticatedIdentityId;
        return true;
    }
}
