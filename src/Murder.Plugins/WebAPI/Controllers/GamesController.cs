using Microsoft.AspNetCore.Mvc;
using Murder.ApplicationGame;
using Murder.ApplicationIdentity;
using Murder.DomainGame;
using Murder.DomainGame.GameAggregate;
using Murder.Plugins.AuthenticationMethod.SessionToken;

namespace Murder.Plugins.WebAPI.Controllers;

[ApiController]
public sealed class GamesController(
    GameService gameService,
    IdentityService identityService,
    AuthenticationService authenticationService
) : ApiControllerBase
{
    private readonly GameService _gameService = gameService;
    private readonly IdentityService _identityService = identityService;
    private readonly AuthenticationService _authenticationService = authenticationService;

    [HttpPost("/games")]
    public IActionResult CreateGame([FromBody] CreateGameRequest request)
    {
        if (!TryGetCurrentIdentity(out var identityId))
        {
            return UnauthorizedProblem();
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ValidationProblemResult("Name is required.");
        }

        var gameId = _gameService.CreateGame(request.Name, ToPlayerId(identityId), Visibility.Private);

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            _gameService.DescribeGame(gameId, request.Description);
        }

        if (request.EndTime is not null)
        {
            _gameService.ChangeEnd(gameId, request.EndTime.Value);
        }

        var game = _gameService.GetGame(gameId)!;

        return Created($"/games/{gameId.Id}", new { game = ToGameDto(game) });
    }

    [HttpGet("/games/{gameId}")]
    public IActionResult GetGame(string gameId)
    {
        if (!TryGetCurrentIdentity(out var identityId))
        {
            return UnauthorizedProblem();
        }

        var id = ToGameId(gameId);
        var game = _gameService.GetGame(id);
        if (game is null)
        {
            return NotFoundProblem(
                "/errors/game-not-found",
                "Game not found",
                $"Game '{gameId}' does not exist."
            );
        }

        var playerId = ToPlayerId(identityId);
        if (!IsParticipant(id, playerId))
        {
            return ForbiddenProblem("You are not participating in this game.");
        }

        return Ok(new { game = ToGameDto(game) });
    }

    [HttpPatch("/games/{gameId}")]
    public IActionResult PatchGame(string gameId, [FromBody] PatchGameRequest request)
    {
        if (!TryGetCurrentIdentity(out var identityId))
        {
            return UnauthorizedProblem();
        }

        if (request.Name is null && request.Description is null && request.EndTime is null)
        {
            return ValidationProblemResult("At least one mutable field is required.");
        }

        var id = ToGameId(gameId);
        var game = _gameService.GetGame(id);
        if (game is null)
        {
            return NotFoundProblem(
                "/errors/game-not-found",
                "Game not found",
                $"Game '{gameId}' does not exist."
            );
        }

        if (!IsAdmin(game, ToPlayerId(identityId)))
        {
            return ForbiddenProblem("Only the game admin can modify this game.");
        }

        if (request.Name is not null)
        {
            _gameService.RenameGame(id, request.Name);
        }

        if (request.Description is not null)
        {
            _gameService.DescribeGame(id, request.Description);
        }

        if (request.EndTime is not null)
        {
            _gameService.ChangeEnd(id, request.EndTime.Value);
        }

        return NoContent();
    }

    [HttpDelete("/games/{gameId}")]
    public IActionResult DeleteGame(string gameId)
    {
        if (!TryGetCurrentIdentity(out var identityId))
        {
            return UnauthorizedProblem();
        }

        var id = ToGameId(gameId);
        var game = _gameService.GetGame(id);
        if (game is null)
        {
            return NotFoundProblem(
                "/errors/game-not-found",
                "Game not found",
                $"Game '{gameId}' does not exist."
            );
        }

        if (!IsAdmin(game, ToPlayerId(identityId)))
        {
            return ForbiddenProblem("Only the game admin can delete this game.");
        }

        _gameService.DeleteGame(id);
        return NoContent();
    }

    [HttpGet("/players/me/games")]
    public IActionResult ListMyGames()
    {
        if (!TryGetCurrentIdentity(out var identityId))
        {
            return UnauthorizedProblem();
        }

        var games = _gameService.ListJoinedGames(ToPlayerId(identityId)) ?? [];
        return Ok(new { games = games.Select(id => id.Id) });
    }

    [HttpPost("/games/{gameId}/join")]
    public IActionResult JoinGame(string gameId)
    {
        if (!TryGetCurrentIdentity(out var identityId))
        {
            return UnauthorizedProblem();
        }

        if (_identityService.IsGuest(identityId))
        {
            return ForbiddenProblem("Guests cannot join additional games.");
        }

        _gameService.JoinGame(ToGameId(gameId), ToPlayerId(identityId));
        return NoContent();
    }

    [HttpPost("/games/{gameId}/leave")]
    public IActionResult LeaveGame(string gameId)
    {
        if (!TryGetCurrentIdentity(out var identityId))
        {
            return UnauthorizedProblem();
        }

        _gameService.LeaveGame(ToGameId(gameId), ToPlayerId(identityId));

        if (_identityService.IsGuest(identityId))
        {
            _authenticationService.RemoveMethod<SessionTokenMethodKey>(identityId);
        }

        return NoContent();
    }

    [HttpPost("/games/{gameId}/start")]
    public IActionResult StartGame(string gameId, [FromBody] StartGameRequest request)
    {
        if (!TryGetCurrentIdentity(out var identityId))
        {
            return UnauthorizedProblem();
        }

        var id = ToGameId(gameId);
        var game = _gameService.GetGame(id);
        if (game is null)
        {
            return NotFoundProblem(
                "/errors/game-not-found",
                "Game not found",
                $"Game '{gameId}' does not exist."
            );
        }

        if (!IsAdmin(game, ToPlayerId(identityId)))
        {
            return ForbiddenProblem("Only the game admin can start this game.");
        }

        _gameService.StartGame(id, request.EndTime);
        return NoContent();
    }

    [HttpPost("/games/{gameId}/end")]
    public IActionResult EndGame(string gameId)
    {
        if (!TryGetCurrentIdentity(out var identityId))
        {
            return UnauthorizedProblem();
        }

        var id = ToGameId(gameId);
        var game = _gameService.GetGame(id);
        if (game is null)
        {
            return NotFoundProblem(
                "/errors/game-not-found",
                "Game not found",
                $"Game '{gameId}' does not exist."
            );
        }

        if (!IsAdmin(game, ToPlayerId(identityId)))
        {
            return ForbiddenProblem("Only the game admin can end this game.");
        }

        _gameService.EndGame(id);
        return NoContent();
    }

    [HttpPatch("/games/{gameId}/end")]
    public IActionResult ChangeEnd(string gameId, [FromBody] ChangeEndRequest request)
    {
        if (!TryGetCurrentIdentity(out var identityId))
        {
            return UnauthorizedProblem();
        }

        if (request.EndTime is null)
        {
            return ValidationProblemResult("endTime is required.");
        }

        var id = ToGameId(gameId);
        var game = _gameService.GetGame(id);
        if (game is null)
        {
            return NotFoundProblem(
                "/errors/game-not-found",
                "Game not found",
                $"Game '{gameId}' does not exist."
            );
        }

        if (!IsAdmin(game, ToPlayerId(identityId)))
        {
            return ForbiddenProblem("Only the game admin can change game end time.");
        }

        _gameService.ChangeEnd(id, request.EndTime.Value);
        return NoContent();
    }

    [HttpGet("/games/{gameId}/victim")]
    public IActionResult Victim(string gameId)
    {
        if (!TryGetCurrentIdentity(out var identityId))
        {
            return UnauthorizedProblem();
        }

        var id = ToGameId(gameId);
        var playerId = ToPlayerId(identityId);

        if (!IsParticipant(id, playerId))
        {
            return ForbiddenProblem("You are not participating in this game.");
        }

        var victimPlayerId = _gameService.Victim(id, playerId);
        return Ok(new { victimPlayerId = victimPlayerId.Id });
    }

    [HttpPost("/games/{gameId}/kills")]
    public IActionResult Kill(string gameId, [FromBody] KillRequest request)
    {
        if (!TryGetCurrentIdentity(out var identityId))
        {
            return UnauthorizedProblem();
        }

        if (string.IsNullOrWhiteSpace(request.VictimPlayerId))
        {
            return ValidationProblemResult("victimPlayerId is required.");
        }

        var id = ToGameId(gameId);
        var playerId = ToPlayerId(identityId);

        if (!IsParticipant(id, playerId))
        {
            return ForbiddenProblem("You are not participating in this game.");
        }

        var nextVictim = _gameService.Kill(id, playerId, new PlayerId(request.VictimPlayerId));
        return Ok(
            new
            {
                nextVictimPlayerId = nextVictim?.Id,
                gameEnded = nextVictim is null,
            }
        );
    }

    [HttpGet("/games/{gameId}/leaderboard")]
    public IActionResult Leaderboard(string gameId)
    {
        if (!TryGetCurrentIdentity(out var identityId))
        {
            return UnauthorizedProblem();
        }

        var id = ToGameId(gameId);
        var playerId = ToPlayerId(identityId);

        if (!IsParticipant(id, playerId))
        {
            return ForbiddenProblem("You are not participating in this game.");
        }

        var entries = _gameService
            .Leaderboard(id)
            .Select(pair => new
            {
                playerId = pair.Key.Id,
                kills = pair.Value,
            });

        return Ok(new { entries });
    }

    [HttpGet("/games/{gameId}/participants")]
    public IActionResult Participants(string gameId)
    {
        if (!TryGetCurrentIdentity(out var identityId))
        {
            return UnauthorizedProblem();
        }

        var id = ToGameId(gameId);
        var playerId = ToPlayerId(identityId);

        if (!IsParticipant(id, playerId))
        {
            return ForbiddenProblem("You are not participating in this game.");
        }

        var participants = _gameService
            .Participants(id)
            .Select(player => _identityService.GetIdentity(ToIdentityId(player)))
            .Select(identity => new
            {
                id = identity.Id.Id,
                name = identity.Name,
                kind = identity is Murder.DomainIdentity.User ? "user" : "guest",
            });

        return Ok(new { participants });
    }

    private bool IsParticipant(GameId gameId, PlayerId playerId)
    {
        var participants = _gameService.Participants(gameId);
        return participants.Contains(playerId);
    }

    private static bool IsAdmin(IReadOnlyGame game, PlayerId playerId)
    {
        return game.Admin is PlayerId adminId && adminId == playerId;
    }

    private static object ToGameDto(IReadOnlyGame game)
    {
        return new
        {
            id = game.Id.Id,
            name = game.Name,
            description = game.Description,
            state = game.State.ToString().ToLowerInvariant(),
            adminPlayerId = game.Admin?.Id,
            startTime = game.StartTime,
            endTime = game.EndTime,
        };
    }

    public sealed record CreateGameRequest(string Name, string? Description, DateTimeOffset? EndTime);

    public sealed record PatchGameRequest(string? Name, string? Description, DateTimeOffset? EndTime);

    public sealed record StartGameRequest(DateTimeOffset? EndTime);

    public sealed record ChangeEndRequest(DateTimeOffset? EndTime);

    public sealed record KillRequest(string VictimPlayerId);
}