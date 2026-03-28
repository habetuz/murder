using Microsoft.AspNetCore.Mvc;
using Murder.ApplicationIdentity;
using Murder.DomainIdentity;
using Murder.Plugins.AuthenticationMethod.Password;
using Murder.Plugins.AuthenticationMethod.SessionToken;
using Murder.Plugins.WebAPI.Authentication;

namespace Murder.Plugins.WebAPI.Controllers;

[ApiController]
[Route("credentials")]
public sealed class CredentialsController(
    AuthenticationService authenticationService,
    IdentityService identityService,
    ICredentialRepository credentialRepository
) : ApiControllerBase
{
    private readonly AuthenticationService _authenticationService = authenticationService;
    private readonly IdentityService _identityService = identityService;
    private readonly ICredentialRepository _credentialRepository = credentialRepository;

    [HttpGet]
    public IActionResult GetCredentials()
    {
        if (!TryGetCurrentIdentity(out var identityId))
        {
            return UnauthorizedProblem();
        }

        var passwordCredentials = _credentialRepository.FindAll<PasswordMethodKey>(identityId).Select(id =>
            new
            {
                id = id.Id,
                type = "password",
                expiresAt = (DateTimeOffset?)null,
            }
        );

        var sessionCredentials = _credentialRepository
            .FindAll<SessionTokenMethodKey>(identityId)
            .Select(id =>
            {
                var stored = _credentialRepository.FindById<SessionTokenMethodKey>(id);
                var expiresAt =
                    stored?.credential is SessionTokenStoredCredential session
                        ? session.ExpiresAtUtc
                        : (DateTimeOffset?)null;

                return new
                {
                    id = id.Id,
                    type = "sessionToken",
                    expiresAt,
                };
            });

        return Ok(passwordCredentials.Concat(sessionCredentials));
    }

    [HttpPost("password")]
    public IActionResult SetPassword([FromBody] SetPasswordRequest request)
    {
        if (!TryGetCurrentIdentity(out var identityId))
        {
            return UnauthorizedProblem();
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return ValidationProblemResult("Password is required.");
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

        if (identity is not Murder.DomainIdentity.User)
        {
            return ForbiddenProblem("Guest accounts cannot manage password credentials.");
        }

        _credentialRepository.DeleteAll<PasswordMethodKey>(identityId);
        _authenticationService.AddMethod(identityId, new PasswordEnrollmentData(request.Password));

        var newCredential = _credentialRepository.FindAll<PasswordMethodKey>(identityId).FirstOrDefault();

        return Created(
            "/credentials/password",
            new
            {
                id = newCredential.Id,
                type = "password",
            }
        );
    }

    [HttpDelete("{credentialId}")]
    public IActionResult DeleteCredential(string credentialId)
    {
        if (!TryGetCurrentIdentity(out var identityId))
        {
            return UnauthorizedProblem();
        }

        var id = new CredentialId(credentialId);
        if (BelongsToIdentity<PasswordMethodKey>(identityId, id) || BelongsToIdentity<SessionTokenMethodKey>(identityId, id))
        {
            _authenticationService.RevokeCredential(id);
            return NoContent();
        }

        return NotFoundProblem(
            "/errors/not-found",
            "Credential not found",
            $"Credential '{credentialId}' does not belong to caller."
        );
    }

    [HttpDelete("session")]
    public IActionResult DeleteSessions()
    {
        if (!TryGetCurrentIdentity(out var identityId))
        {
            return UnauthorizedProblem();
        }

        _authenticationService.RemoveMethod<SessionTokenMethodKey>(identityId);
        SessionCookie.Expire(Response, Request);
        return NoContent();
    }

    private bool BelongsToIdentity<TMethod>(IdentityId identityId, CredentialId credentialId)
        where TMethod : IAuthenticationMethodKey
    {
        var stored = _credentialRepository.FindById<TMethod>(credentialId);
        return stored is not null && stored.Value.identity == identityId;
    }

    public sealed record SetPasswordRequest(string Password);
}