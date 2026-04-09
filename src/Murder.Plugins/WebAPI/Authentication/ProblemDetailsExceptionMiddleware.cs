using Murder.ApplicationGame;
using Murder.DomainGame.GameAggregate;
using Murder.DomainIdentity;

namespace Murder.Plugins.WebAPI.Authentication;

public sealed class ProblemDetailsExceptionMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not StackOverflowException)
        {
            await WriteProblem(context, ex);
        }
    }

    private static Task WriteProblem(HttpContext context, Exception exception)
    {
        (int status, string type, string title) = exception switch
        {
            GameNotFoundException => (
                StatusCodes.Status404NotFound,
                "/errors/game-not-found",
                "Game not found"
            ),
            UnexpectedGameStateException => (
                StatusCodes.Status409Conflict,
                "/errors/invalid-game-state",
                "Invalid game state"
            ),
            NotEnoughParticipantsException => (
                StatusCodes.Status409Conflict,
                "/errors/not-enough-participants",
                "Not enough participants"
            ),
            PlayerNotParticipating => (
                StatusCodes.Status403Forbidden,
                "/errors/not-participant",
                "Not a participant"
            ),
            PlayerDeadException => (
                StatusCodes.Status409Conflict,
                "/errors/player-dead",
                "Player is dead"
            ),
            IncorrectVictimException => (
                StatusCodes.Status409Conflict,
                "/errors/incorrect-victim",
                "Incorrect victim"
            ),
            NoMoreVictimsException => (
                StatusCodes.Status409Conflict,
                "/errors/no-more-victims",
                "No more victims"
            ),
            UserGuestMismatchException => (
                StatusCodes.Status409Conflict,
                "/errors/identity-kind-mismatch",
                "Identity kind mismatch"
            ),
            DuplicateUsernameException => (
                StatusCodes.Status409Conflict,
                "/errors/name-conflict",
                "Name conflict"
            ),
            DuplicateDisplayNameException => (
                StatusCodes.Status409Conflict,
                "/errors/name-conflict",
                "Name conflict"
            ),
            AuthenticationMethodMismatchException => (
                StatusCodes.Status400BadRequest,
                "/errors/unsupported-method",
                "Unsupported method"
            ),
            KeyNotFoundException => (
                StatusCodes.Status404NotFound,
                "/errors/not-found",
                "Not found"
            ),
            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "/errors/unauthorized",
                "Unauthorized"
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                "/errors/internal",
                "Internal server error"
            ),
        };

        context.Response.Clear();
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        return Results
            .Problem(
                statusCode: status,
                type: type,
                title: title,
                detail: exception.Message,
                instance: context.Request.Path
            )
            .ExecuteAsync(context);
    }
}