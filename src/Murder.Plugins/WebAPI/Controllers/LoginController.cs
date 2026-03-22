using Microsoft.AspNetCore.Mvc;
using Murder.ApplicationIdentity;
using Murder.DomainIdentity;
using Murder.Plugins.AuthenticationMethod.Password;
using Murder.Plugins.AuthenticationMethod.SessionToken;
using Murder.Plugins.WebAPI.Authentication;

namespace Murder.Plugins.WebAPI.Controllers;

[ApiController]
public class LoginController(
    AuthenticationService authenticationService,
    IIdentityRepository identityRepository
) : ControllerBase
{
    private readonly AuthenticationService _authenticationService = authenticationService;
    private readonly IIdentityRepository _identityRepository = identityRepository;

    [HttpPost("/login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (!string.Equals(request.LoginType, "password", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "Unsupported login type." });
        }

        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { error = "Username and password are required." });
        }

        IdentityId identityId;

        try
        {
            identityId = _identityRepository.IdentityOfName(request.Username);
        }
        catch (KeyNotFoundException)
        {
            return Unauthorized();
        }
        catch (InvalidOperationException)
        {
            return Unauthorized();
        }

        var authenticatedIdentityId = _authenticationService.Authenticate<PasswordMethodKey>(
            new PasswordIncomingCredential(identityId, request.Password)
        );

        if (authenticatedIdentityId is null)
        {
            return Unauthorized();
        }

        var sessionToken = _authenticationService.AddMethod<SessionTokenMethodKey>(
            authenticatedIdentityId.Value,
            new SessionTokenEnrollmentData(authenticatedIdentityId.Value)
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

        return Ok();
    }

    public sealed record LoginRequest(string LoginType, string Username, string Password);
}
