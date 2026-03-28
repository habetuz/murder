using Microsoft.AspNetCore.Mvc;
using Murder.DomainGame;
using Murder.DomainIdentity;
using Murder.Plugins.WebAPI.Authentication;

namespace Murder.Plugins.WebAPI.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected bool TryGetCurrentIdentity(out IdentityId identityId)
    {
        return AuthenticationContextExtensions.TryGetAuthenticatedIdentity(HttpContext, out identityId);
    }

    protected static PlayerId ToPlayerId(IdentityId identityId)
    {
        return new PlayerId(identityId.Id);
    }

    protected static IdentityId ToIdentityId(PlayerId playerId)
    {
        return new IdentityId(playerId.Id);
    }

    protected static GameId ToGameId(string gameId)
    {
        return new GameId(gameId);
    }

    protected ObjectResult UnauthorizedProblem(string detail = "Authentication is required.")
    {
        return Problem(
            type: "/errors/unauthorized",
            title: "Unauthorized",
            detail: detail,
            statusCode: StatusCodes.Status401Unauthorized
        );
    }

    protected ObjectResult ForbiddenProblem(string detail = "You are not allowed to perform this action.")
    {
        return Problem(
            type: "/errors/forbidden",
            title: "Forbidden",
            detail: detail,
            statusCode: StatusCodes.Status403Forbidden
        );
    }

    protected ObjectResult ValidationProblemResult(string detail)
    {
        return Problem(
            type: "/errors/validation",
            title: "Validation failure",
            detail: detail,
            statusCode: StatusCodes.Status400BadRequest
        );
    }

    protected ObjectResult UnsupportedMethodProblem(string detail)
    {
        return Problem(
            type: "/errors/unsupported-method",
            title: "Unsupported method",
            detail: detail,
            statusCode: StatusCodes.Status400BadRequest
        );
    }

    protected ObjectResult NotFoundProblem(string type, string title, string detail)
    {
        return Problem(type: type, title: title, detail: detail, statusCode: StatusCodes.Status404NotFound);
    }

    protected static object ToPlayerRef(Identity identity)
    {
        return new
        {
            id = identity.Id.Id,
            name = identity.Name,
            kind = identity is User ? "user" : "guest",
        };
    }
}