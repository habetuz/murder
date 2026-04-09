using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Murder.ApplicationGame;
using Murder.ApplicationIdentity;
using Murder.DomainGame;
using Murder.DomainGame.GameAggregate;
using Murder.Plugins.WebAPI.Authentication;
using Murder.Plugins.WebAPI.Watch;

namespace Murder.Plugins.WebAPI.Controllers;

[ApiController]
public sealed class WatchController(
    GameService gameService,
    IdentityService identityService,
    GameEventBus eventBus,
    PendingKillStore pendingKillStore
) : ApiControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private static readonly TimeSpan KeepaliveInterval = TimeSpan.FromSeconds(15);

    [HttpGet("/games/{gameId}/watch")]
    public async Task Watch(string gameId, CancellationToken cancellationToken)
    {
        // Auth check
        if (!TryGetCurrentIdentity(out var identityId))
        {
            await WriteProblem(StatusCodes.Status401Unauthorized, "/errors/unauthorized", "Unauthorized", "Authentication is required.");
            return;
        }

        var gid = ToGameId(gameId);
        var playerId = ToPlayerId(identityId);

        var game = gameService.GetGame(gid);
        if (game is null)
        {
            await WriteProblem(StatusCodes.Status404NotFound, "/errors/game-not-found", "Game not found", $"Game '{gameId}' does not exist.");
            return;
        }

        if (!gameService.Participants(gid).Contains(playerId))
        {
            await WriteProblem(StatusCodes.Status403Forbidden, "/errors/not-participant", "Not a participant", "You are not participating in this game.");
            return;
        }

        // Switch to SSE
        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Connection = "keep-alive";
        Response.StatusCode = StatusCodes.Status200OK;
        await Response.Body.FlushAsync(cancellationToken);

        var channel = eventBus.Subscribe(gid);
        try
        {
            // Send initial SYNC
            var payload = BuildPayload(gid, playerId);
            await WriteSseEvent("SYNC", payload, cancellationToken);

            // Event loop
            while (!cancellationToken.IsCancellationRequested)
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(KeepaliveInterval);

                GameEvent evt;
                bool received;
                try
                {
                    received = await channel.Reader.WaitToReadAsync(cts.Token);
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    // Keepalive timeout — send comment
                    await WriteSseComment("keepalive", cancellationToken);
                    continue;
                }

                if (!received)
                {
                    break; // Channel completed (unsubscribed)
                }

                while (channel.Reader.TryRead(out evt))
                {
                    if (evt == GameEvent.Deleted)
                    {
                        await WriteSseEvent("GAME_DELETED", new { }, cancellationToken);
                        return;
                    }
                }

                // Coalesced updates — build fresh snapshot
                var updatePayload = BuildPayload(gid, playerId);
                if (updatePayload is null)
                {
                    // Game was deleted between event and payload build
                    await WriteSseEvent("GAME_DELETED", new { }, cancellationToken);
                    return;
                }

                await WriteSseEvent("UPDATE", updatePayload, cancellationToken);
            }
        }
        finally
        {
            eventBus.Unsubscribe(gid, channel);
        }
    }

    private object? BuildPayload(GameId gameId, PlayerId playerId)
    {
        var game = gameService.GetGame(gameId);
        if (game is null)
        {
            return null;
        }

        var names = gameService.ParticipantNames(gameId);
        var participants = gameService
            .Participants(gameId)
            .Select(pid =>
            {
                var identity = identityService.GetIdentity(ToIdentityId(pid));
                return new
                {
                    id = pid.Id,
                    name = names[pid],
                    username = (identity as Murder.DomainIdentity.User)?.Name,
                    kind = identity is Murder.DomainIdentity.User ? "user" : "guest",
                };
            })
            .ToArray();

        object? leaderboard = null;
        if (game.State is GameState.Running or GameState.Ended)
        {
            leaderboard = gameService
                .Leaderboard(gameId)
                .Select(pair => new { playerId = pair.Key.Id, name = names[pair.Key], kills = pair.Value })
                .ToArray();
        }

        var me = BuildPlayerState(game, gameId, playerId);

        return new
        {
            game = new
            {
                id = game.Id.Id,
                name = game.Name,
                description = game.Description,
                state = game.State.ToString().ToLowerInvariant(),
                adminPlayerId = game.Admin?.Id,
                startTime = game.StartTime,
                endTime = game.EndTime,
            },
            participants,
            leaderboard,
            me,
        };
    }

    private object BuildPlayerState(IReadOnlyGame game, GameId gameId, PlayerId playerId)
    {
        if (game.State != GameState.Running)
        {
            return new
            {
                victimPlayerId = (string?)null,
                victimName = (string?)null,
                alive = game.State == GameState.Pending || !IsPlayerDead(gameId, playerId),
                pendingKill = false,
                pendingKillSent = false,
            };
        }

        var names = gameService.ParticipantNames(gameId);
        var hasPendingKillAsVictim = pendingKillStore.HasPendingKillAsVictim(gameId, playerId);
        var hasPendingKillAsKiller = pendingKillStore.GetForKiller(gameId, playerId) is not null;

        // Running state — try to get victim
        try
        {
            var victimId = gameService.Victim(gameId, playerId);
            return new
            {
                victimPlayerId = (string?)victimId.Id,
                victimName = (string?)names[victimId],
                alive = true,
                pendingKill = hasPendingKillAsVictim,
                pendingKillSent = hasPendingKillAsKiller,
            };
        }
        catch (PlayerDeadException)
        {
            return new
            {
                victimPlayerId = (string?)null,
                victimName = (string?)null,
                alive = false,
                pendingKill = false,
                pendingKillSent = false,
            };
        }
        catch (NoMoreVictimsException)
        {
            return new
            {
                victimPlayerId = (string?)null,
                victimName = (string?)null,
                alive = true,
                pendingKill = false,
                pendingKillSent = false,
            };
        }
    }

    private bool IsPlayerDead(GameId gameId, PlayerId playerId)
    {
        try
        {
            gameService.Victim(gameId, playerId);
            return false;
        }
        catch (PlayerDeadException)
        {
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task WriteSseEvent(string eventType, object data, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);
        var message = $"event: {eventType}\ndata: {json}\n\n";
        await Response.WriteAsync(message, ct);
        await Response.Body.FlushAsync(ct);
    }

    private async Task WriteSseComment(string comment, CancellationToken ct)
    {
        await Response.WriteAsync($": {comment}\n\n", ct);
        await Response.Body.FlushAsync(ct);
    }

    private async Task WriteProblem(int statusCode, string type, string title, string detail)
    {
        Response.StatusCode = statusCode;
        Response.ContentType = "application/problem+json";
        var problem = new { type, title, status = statusCode, detail };
        await Response.WriteAsJsonAsync(problem);
    }
}
