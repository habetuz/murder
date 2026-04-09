using Microsoft.AspNetCore.Mvc;
using Murder.ApplicationGame;
using Murder.ApplicationIdentity;
using Murder.DomainGame;
using Murder.DomainGame.GameAggregate;
using Murder.Plugins.AuthenticationMethod.SessionToken;
using Murder.Plugins.WebAPI.Watch;

namespace Murder.Plugins.WebAPI.Controllers;

[ApiController]
public sealed class GamesController(
    GameService gameService,
    IdentityService identityService,
    AuthenticationService authenticationService,
    GameEventBus eventBus,
    PendingKillStore pendingKillStore
) : ApiControllerBase
{
    private readonly GameService _gameService = gameService;
    private readonly IdentityService _identityService = identityService;
    private readonly AuthenticationService _authenticationService = authenticationService;
    private readonly GameEventBus _eventBus = eventBus;
    private readonly PendingKillStore _pendingKillStore = pendingKillStore;

    [HttpPost("/games")]
    public IActionResult CreateGame([FromBody] CreateGameRequest request)
    {
        if (!TryGetCurrentIdentity(out var identityId))
        {
            return UnauthorizedProblem();
        }

        if (_identityService.IsGuest(identityId))
        {
            return ForbiddenProblem("Guests cannot create games.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ValidationProblemResult("Name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return ValidationProblemResult("DisplayName is required.");
        }

        var gameId = _gameService.CreateGame(request.Name, ToPlayerId(identityId), request.DisplayName, Visibility.Private);

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

        _eventBus.Notify(id);
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
        _eventBus.NotifyDeleted(id);
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
    public IActionResult JoinGame(string gameId, [FromBody] JoinGameRequest request)
    {
        if (!TryGetCurrentIdentity(out var identityId))
        {
            return UnauthorizedProblem();
        }

        if (_identityService.IsGuest(identityId))
        {
            return ForbiddenProblem("Guests cannot join additional games.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ValidationProblemResult("Name is required.");
        }

        _gameService.JoinGame(ToGameId(gameId), ToPlayerId(identityId), request.Name);
        _eventBus.Notify(ToGameId(gameId));
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

        _eventBus.Notify(ToGameId(gameId));
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

        try
        {
            _gameService.StartGame(id, request.EndTime);
        }
        catch (Murder.DomainGame.GameAggregate.NotEnoughParticipantsException ex)
        {
            return ValidationProblemResult(ex.Message);
        }

        _eventBus.Notify(id);
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
        _eventBus.Notify(id);
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
        _eventBus.Notify(id);
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
        var names = _gameService.ParticipantNames(id);
        return Ok(new { victimPlayerId = victimPlayerId.Id, victimName = names[victimPlayerId] });
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

        // Validate the kill is correct (right victim, player alive, game running)
        // This calls Victim() which checks all preconditions
        var correctVictim = _gameService.Victim(id, playerId);
        if (correctVictim != new PlayerId(request.VictimPlayerId))
        {
            return ValidationProblemResult("Incorrect victim.");
        }

        // Create a pending kill — don't execute yet
        _pendingKillStore.Add(id, playerId, correctVictim);
        _eventBus.Notify(id);

        return Ok(new { status = "pending" });
    }

    [HttpPost("/games/{gameId}/kills/respond")]
    public IActionResult RespondToKill(string gameId, [FromBody] KillRespondRequest request)
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

        // The responding player must be the victim of a pending kill
        var pending = _pendingKillStore.RemoveForVictim(id, playerId);
        if (pending is null)
        {
            return NotFoundProblem(
                "/errors/no-pending-kill",
                "No pending kill",
                "There is no pending kill claim against you."
            );
        }

        if (request.Accepted)
        {
            // Execute the actual kill
            var nextVictim = _gameService.Kill(id, pending.Killer, pending.Victim);
            _eventBus.Notify(id);

            return Ok(new { result = "confirmed" });
        }
        else
        {
            // Kill denied — just notify so killer sees the denial
            _eventBus.Notify(id);
            return Ok(new { result = "denied" });
        }
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

        var names = _gameService.ParticipantNames(id);
        var entries = _gameService
            .Leaderboard(id)
            .Select(pair => new
            {
                playerId = pair.Key.Id,
                name = names[pair.Key],
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

        var names = _gameService.ParticipantNames(id);
        var participants = _gameService
            .Participants(id)
            .Select(player =>
            {
                var identity = _identityService.GetIdentity(ToIdentityId(player));
                return new
                {
                    id = player.Id,
                    name = names[player],
                    username = (identity as Murder.DomainIdentity.User)?.Name,
                    kind = identity is Murder.DomainIdentity.User ? "user" : "guest",
                };
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

    public sealed record CreateGameRequest(string Name, string DisplayName, string? Description, DateTimeOffset? EndTime);

    public sealed record PatchGameRequest(string? Name, string? Description, DateTimeOffset? EndTime);

    public sealed record StartGameRequest(DateTimeOffset? EndTime);

    public sealed record ChangeEndRequest(DateTimeOffset? EndTime);

    public sealed record JoinGameRequest(string Name);

    public sealed record KillRequest(string VictimPlayerId);

    public sealed record KillRespondRequest(bool Accepted);
}