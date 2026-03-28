using Microsoft.AspNetCore.Mvc;
using Murder.ApplicationIdentity;
using Murder.DomainIdentity;
using Murder.Plugins.AuthenticationMethod.Password;
using Murder.Plugins.AuthenticationMethod.SessionToken;
using Murder.Plugins.WebAPI.Authentication;

namespace Murder.Plugins.WebAPI.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController(
    AuthenticationService authenticationService,
    IdentityService identityService,
    IIdentityRepository identityRepository,
    ICredentialRepository credentialRepository
) : ApiControllerBase
{
    private readonly AuthenticationService _authenticationService = authenticationService;
    private readonly IdentityService _identityService = identityService;
    private readonly IIdentityRepository _identityRepository = identityRepository;
    private readonly ICredentialRepository _credentialRepository = credentialRepository;

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (!string.Equals(request.Type, "password", StringComparison.OrdinalIgnoreCase))
        {
            return UnsupportedMethodProblem("Unsupported login type.");
        }

        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return ValidationProblemResult("Username and password are required.");
        }

        IdentityId identityId;
        try
        {
            identityId = _identityRepository.IdentityOfName(request.Username);
        }
        catch (KeyNotFoundException)
        {
            return UnauthorizedProblem("Credentials are invalid.");
        }
        catch (InvalidOperationException)
        {
            return UnauthorizedProblem("Credentials are invalid.");
        }

        var authenticatedIdentity = _authenticationService.Authenticate<PasswordMethodKey>(
            new PasswordIncomingCredential(identityId, request.Password)
        );

        if (authenticatedIdentity is null)
        {
            return UnauthorizedProblem("Credentials are invalid.");
        }

        var sessionToken = _authenticationService.AddMethod<SessionTokenMethodKey>(
            authenticatedIdentity.Value,
            new SessionTokenEnrollmentData(authenticatedIdentity.Value)
        );

        if (string.IsNullOrWhiteSpace(sessionToken))
        {
            return Problem(
                type: "/errors/internal",
                title: "Internal server error",
                detail: "Failed to create session token.",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }

        SessionCookie.Append(Response, Request, sessionToken);
        return Ok();
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        if (!TryGetCurrentIdentity(out var identityId))
        {
            return UnauthorizedProblem();
        }

        _authenticationService.RemoveMethod<SessionTokenMethodKey>(identityId);
        SessionCookie.Expire(Response, Request);
        return NoContent();
    }

    [HttpGet("session")]
    public IActionResult Session()
    {
        if (!TryGetCurrentIdentity(out var identityId))
        {
            return UnauthorizedProblem();
        }

        Identity identity;
        try
        {
            identity = _identityService.GetIdentity(identityId);
        }
        catch (KeyNotFoundException)
        {
            return UnauthorizedProblem();
        }

        var expiresAt = _credentialRepository
            .FindAll<SessionTokenMethodKey>(identityId)
            .Select(id => _credentialRepository.FindById<SessionTokenMethodKey>(id))
            .Where(stored => stored is not null)
            .Select(stored => stored!.Value.credential)
            .OfType<SessionTokenStoredCredential>()
            .Select(credential => (DateTimeOffset?)credential.ExpiresAtUtc)
            .OrderByDescending(value => value)
            .FirstOrDefault();

        return Ok(
            new
            {
                player = ToPlayerRef(identity),
                session = new
                {
                    expiresAt,
                },
            }
        );
    }

    public sealed record LoginRequest(string Type, string Username, string Password);
}