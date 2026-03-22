using Microsoft.AspNetCore.Mvc;
using Murder.ApplicationIdentity;
using Murder.DomainIdentity;
using Murder.Plugins.AuthenticationMethod.Password;
using Murder.Plugins.AuthenticationMethod.SessionToken;
using Murder.Plugins.WebAPI.Authentication;

namespace Murder.Plugins.WebAPI.Controllers;

[ApiController]
public class CredentialController(
    AuthenticationService authenticationService,
    IdentityService identityService,
    ICredentialRepository credentialRepository
) : ControllerBase
{
    private readonly AuthenticationService _authenticationService = authenticationService;
    private readonly IdentityService _identityService = identityService;
    private readonly ICredentialRepository _credentialRepository = credentialRepository;

    [HttpPost("/credential")]
    public IActionResult CreateCredential([FromBody] CreateCredentialRequest request)
    {
        if (!TryGetAuthenticatedUser(out var user))
        {
            return Unauthorized();
        }

        if (!string.Equals(request.Type, "password", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "Unsupported credential type." });
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { error = "Password is required." });
        }

        var newCredential = EnrollPasswordCredential(user.Id, request.Password);

        if (string.IsNullOrWhiteSpace(newCredential.Id))
        {
            return Problem("Credential was created but could not be identified.");
        }

        return Created(
            $"/credential/{newCredential.Id}",
            new
            {
                id = newCredential.Id,
                type = "password",
            }
        );
    }

    [HttpDelete("/credential/{credentialId}")]
    public IActionResult DeleteCredential(string credentialId)
    {
        if (!TryGetAuthenticatedUser(out var user))
        {
            return Unauthorized();
        }

        var id = new CredentialId(credentialId);

        if (BelongsToAuthenticatedUser<PasswordMethodKey>(user.Id, id)
            || BelongsToAuthenticatedUser<SessionTokenMethodKey>(user.Id, id))
        {
            _authenticationService.RevokeCredential(id);
            return NoContent();
        }

        return NotFound();
    }

    [HttpGet("/credential")]
    public IActionResult ListCredentials()
    {
        if (!TryGetAuthenticatedUser(out var user))
        {
            return Unauthorized();
        }

        var passwordCredentials = _credentialRepository
            .FindAll<PasswordMethodKey>(user.Id)
            .Select(id => new
            {
                id = id.Id,
                type = "password",
                expiresAtUtc = (DateTimeOffset?)null,
            });

        var sessionCredentials = _credentialRepository
            .FindAll<SessionTokenMethodKey>(user.Id)
            .Select(id =>
            {
                var stored = _credentialRepository.FindById<SessionTokenMethodKey>(id);
                var expiresAtUtc =
                    stored?.credential is SessionTokenStoredCredential tokenCredential
                        ? tokenCredential.ExpiresAtUtc
                        : (DateTimeOffset?)null;

                return new
                {
                    id = id.Id,
                    type = "sessionToken",
                    expiresAtUtc,
                };
            });

        return Ok(passwordCredentials.Concat(sessionCredentials));
    }

    private bool TryGetAuthenticatedUser(out User user)
    {
        user = null!;

        if (!AuthenticationContextExtensions.TryGetAuthenticatedIdentity(HttpContext, out var identityId))
        {
            return false;
        }

        Identity identity;

        try
        {
            identity = _identityService.GetIdentity(identityId);
        }
        catch (KeyNotFoundException)
        {
            return false;
        }

        if (identity is not User authenticatedUser)
        {
            return false;
        }

        user = authenticatedUser;
        return true;
    }

    private bool BelongsToAuthenticatedUser<TMethod>(IdentityId userId, CredentialId credentialId)
        where TMethod : IAuthenticationMethodKey
    {
        var stored = _credentialRepository.FindById<TMethod>(credentialId);
        return stored is not null && stored.Value.identity == userId;
    }

    private CredentialId EnrollPasswordCredential(IdentityId userId, string password)
    {
        _credentialRepository.DeleteAll<PasswordMethodKey>(userId);

        _authenticationService.AddMethod(
            userId,
            new PasswordEnrollmentData(password)
        );

        var updated = _credentialRepository.FindAll<PasswordMethodKey>(userId);
        return updated.FirstOrDefault();
    }

    public sealed record CreateCredentialRequest(string Type, string Password);
}
