using Microsoft.AspNetCore.Mvc;
using Murder.ApplicationGame;
using Murder.ApplicationIdentity;
using Murder.DomainGame;
using Murder.DomainIdentity;
using Murder.Plugins.AuthenticationMethod.SessionToken;
using Murder.Plugins.WebAPI.Authentication;

namespace Murder.Plugins.WebAPI.Controllers;

[ApiController]
public sealed class IdentityController(
    IdentityService identityService,
    AuthenticationService authenticationService,
    GameService gameService
) : ApiControllerBase
{
    private readonly IdentityService _identityService = identityService;
    private readonly AuthenticationService _authenticationService = authenticationService;
    private readonly GameService _gameService = gameService;

    [HttpPost("/users")]
    public IActionResult CreateUser([FromBody] CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ValidationProblemResult("Name is required.");
        }

        var identityId = _identityService.CreateUser(request.Name);
        var sessionToken = _authenticationService.AddMethod<SessionTokenMethodKey>(
            identityId,
            new SessionTokenEnrollmentData(identityId)
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

        return Created(
            $"/players/{identityId.Id}",
            new
            {
                player = new
                {
                    id = identityId.Id,
                    name = request.Name,
                    kind = "user",
                    state = UserState.Active.ToString().ToLowerInvariant(),
                },
            }
        );
    }

    [HttpPost("/guests")]
    public IActionResult CreateGuest([FromBody] CreateGuestRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.GameId))
        {
            return ValidationProblemResult("Name and gameId are required.");
        }

        var guestIdentityId = _identityService.CreateGuest(request.Name);
        _gameService.JoinGame(new GameId(request.GameId), new PlayerId(guestIdentityId.Id));

        var sessionToken = _authenticationService.AddMethod<SessionTokenMethodKey>(
            guestIdentityId,
            new SessionTokenEnrollmentData(guestIdentityId)
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

        return Created(
            $"/players/{guestIdentityId.Id}",
            new
            {
                player = new
                {
                    id = guestIdentityId.Id,
                    name = request.Name,
                    kind = "guest",
                },
                joinedGameId = request.GameId,
            }
        );
    }

    [HttpGet("/players/me")]
    public IActionResult GetMe()
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

        return Ok(new { player = ToPlayerProfile(identity) });
    }

    [HttpGet("/players/{playerId}")]
    public IActionResult GetPlayer(string playerId)
    {
        if (!TryGetCurrentIdentity(out _))
        {
            return UnauthorizedProblem();
        }

        Identity identity;
        try
        {
            identity = _identityService.GetIdentity(new IdentityId(playerId));
        }
        catch (KeyNotFoundException)
        {
            return NotFoundProblem(
                "/errors/not-found",
                "Player not found",
                $"Player '{playerId}' was not found."
            );
        }

        return Ok(new { player = ToPlayerProfile(identity) });
    }

    [HttpDelete("/users/me")]
    public IActionResult DeactivateSelf()
    {
        if (!TryGetCurrentIdentity(out var identityId))
        {
            return UnauthorizedProblem();
        }

        _identityService.DeactivateUser(identityId);
        return NoContent();
    }

    private static object ToPlayerProfile(Identity identity)
    {
        return new
        {
            id = identity.Id.Id,
            name = identity.Name,
            kind = identity is User ? "user" : "guest",
            state = identity is User user ? user.State.ToString().ToLowerInvariant() : null,
        };
    }

    public sealed record CreateUserRequest(string Name);

    public sealed record CreateGuestRequest(string Name, string GameId);
}