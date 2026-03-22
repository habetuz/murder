using Microsoft.AspNetCore.Mvc;
using Murder.ApplicationIdentity;
using Murder.DomainIdentity;
using Murder.Plugins.WebAPI.Authentication;

namespace Murder.Plugins.WebAPI.Controllers;

[ApiController]
public class SelfController(IdentityService identityService) : ControllerBase
{
    private readonly IdentityService _identityService = identityService;

    [HttpGet("/self")]
    public IActionResult GetSelf()
    {
        if (!AuthenticationContextExtensions.TryGetAuthenticatedIdentity(HttpContext, out var identityId))
        {
            return Unauthorized();
        }

        Identity identity;

        try
        {
            identity = _identityService.GetIdentity(identityId);
        }
        catch (KeyNotFoundException)
        {
            return Unauthorized();
        }

        return Ok(
            new
            {
                id = identity.Id.Id,
                name = identity.Name,
                kind = identity is User ? "user" : "guest",
                state = identity is User user ? user.State.ToString() : null,
            }
        );
    }
}
