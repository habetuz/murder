using Microsoft.AspNetCore.Mvc;
using Murder.ApplicationIdentity;
using Murder.DomainIdentity;
using Murder.Plugins.AuthenticationMethod.SessionToken;
using Murder.Plugins.WebAPI.Authentication;

namespace Murder.Plugins.WebAPI.Controllers;

[ApiController]
public class UserController(
    IdentityService identityService,
    AuthenticationService authenticationService,
    IIdentityRepository identityRepository
) : ControllerBase
{
    private readonly IdentityService _identityService = identityService;
    private readonly AuthenticationService _authenticationService = authenticationService;
    private readonly IIdentityRepository _identityRepository = identityRepository;

    [HttpPost("/user")]
    public IActionResult CreateUser([FromBody] CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return BadRequest(new { error = "Username is required." });
        }

        try
        {
            _identityRepository.IdentityOfName(request.Username);
            return Conflict(new { error = "Username is already taken." });
        }
        catch (KeyNotFoundException)
        {
            // Expected when username does not exist yet.
        }

        var identityId = _identityService.CreateUser(request.Username);

        var sessionToken = _authenticationService.AddMethod<SessionTokenMethodKey>(
            identityId,
            new SessionTokenEnrollmentData(identityId)
        );

        if (string.IsNullOrWhiteSpace(sessionToken))
        {
            return Problem("Failed to create a session token.");
        }

        Response.Cookies.Append(
            AuthenticationCookieNames.SessionToken,
            sessionToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.Add(AuthenticationSettings.SessionTokenLifetime),
                IsEssential = true,
                Path = "/",
            }
        );

        return Created(
            $"/user/{identityId.Id}",
            new
            {
                id = identityId.Id,
                name = request.Username,
                kind = "user",
                state = UserState.Active.ToString(),
            }
        );
    }

    [HttpGet("/user/{id}")]
    public IActionResult GetUser(string id)
    {
        Identity identity;

        try
        {
            identity = _identityService.GetIdentity(new IdentityId(id));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        if (identity is not User user)
        {
            return NotFound();
        }

        return Ok(
            new
            {
                id = user.Id.Id,
                name = user.Name,
                kind = "user",
                state = user.State.ToString(),
            }
        );
    }

    public sealed record CreateUserRequest(string Username);
}
