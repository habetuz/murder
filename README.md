# Murder

A group game large groups.

## Domain

**Guest**

Single game user without login information
- name
 
**User**

Permanent user with login information
- name
- games
- login (adapter)

**Player**

Per game instance of a guest or user that is participating in a game
- kills

**Admin**

player administrating a game. Each game has one administrator

**Game**

A single round of murder
- id
- player(s)
- admin
- murderChain
- startTime
- endTime

**MurderChain**

Responsible for keeping track of murder to victim assignments
- assignments

## HTTP API Design (Implementation Target)

This section defines the target HTTP contract for implementing WebAPI controllers around:
- `IdentityService`
- `AuthenticationService`
- `GameService`

The current controllers are a partial implementation and may change to match this contract.

### Scope and Decisions

- Auth transport: session cookie only.
- No URI version prefix for now.
- API-facing IDs are player-facing IDs only.
- Internal `IdentityId` to `PlayerId` context mapping remains a backend concern.
- Guest creation requires selecting the initial game in the same request.
- Guests cannot join additional games after creation.
- Guest leave is equivalent to guest self-deletion.
- All games are private. Ignore public/private discovery in API behavior.
- Errors follow RFC 9457 Problem Details (`application/problem+json`).

### Base Conventions

- Base path: `/`
- Content type: `application/json`
- Error content type: `application/problem+json`
- Date-time format: ISO 8601 / RFC 3339 UTC timestamps
- IDs: opaque strings (do not infer semantics)
- Cookie name: `session_token`
- Session cookie flags:
	- `HttpOnly=true`
	- `SameSite=Strict`
	- `Path=/`
	- `Secure=true` on HTTPS
	- Rolling expiration on authenticated requests

### Identity Model at API Boundary

Although domains internally distinguish identity and player, the API exposes one player-facing identifier:

```
PlayerRef {
	id: string,
	name: string,
	kind: "user" | "guest"
}
```

Implementation note:
- Backend resolves `IdentityId <-> PlayerId` when calling domain services.
- For one player-facing ID there is always one mapped identity.

## Authorization Model

Authorization is endpoint-specific and evaluates one of these roles:

- Anonymous: no authenticated session
- Authenticated: valid session cookie
- Identity owner: authenticated player acting on own profile/credentials
- Participant: authenticated player currently participating in game
- Game admin: participant that equals game admin

If authenticated but not allowed, return `403 Forbidden`.
If not authenticated, return `401 Unauthorized`.

### Authorization Matrix

| Endpoint Group | Anonymous | Authenticated | Identity Owner | Participant | Game Admin |
|---|---:|---:|---:|---:|---:|
| Register / Login | Yes | Yes | n/a | n/a | n/a |
| Logout / Session introspection | No | Yes | Yes | n/a | n/a |
| Read own profile | No | Yes | Yes | n/a | n/a |
| Deactivate or delete self | No | Yes | Yes (self) | n/a | Optional admin override |
| Manage own credentials | No | Yes | Yes | n/a | n/a |
| Create game | No | Yes | n/a | n/a | n/a |
| Read game metadata | No | Yes | n/a | Participant only | Game admin implicitly |
| Join/Leave game | No | Yes | n/a | Self-participation | Can leave/remove other participants |
| Game gameplay (victim/kill) | No | Yes | n/a | Yes | n/a |
| Game admin mutations (rename, describe, start/end/delete, change end) | No | Yes | n/a | No | Yes |
| Participants/leaderboard | No | Yes | n/a | Yes | Yes |

## Endpoint Reference

### Authentication and Session

### POST /auth/login

Authenticates a player and issues session cookie.

Request:
```json
{
	"type": "password",
	"username": "alice",
	"password": "correct horse battery staple"
}
```

Responses:
- `200 OK` and `Set-Cookie: session_token=...`
- `401 Unauthorized` when credentials are invalid
- `400 Bad Request` for unsupported login type or malformed payload

### POST /auth/logout

Revokes current session credential and expires cookie.

Responses:
- `204 No Content`
- `401 Unauthorized` when no valid session

### GET /auth/session

Returns current session principal.

Response `200 OK`:
```json
{
	"player": {
		"id": "ply_abc123",
		"name": "alice",
		"kind": "user"
	},
	"session": {
		"expiresAt": "2026-03-28T10:15:00Z"
	}
}
```

Responses:
- `401 Unauthorized` when not logged in

### Identity

### POST /users

Creates a permanent user and immediately issues session cookie.

Request:
```json
{
	"name": "alice"
}
```

Response `201 Created`:
```json
{
	"player": {
		"id": "ply_abc123",
		"name": "alice",
		"kind": "user",
		"state": "active"
	}
}
```

Errors:
- `409 Conflict` if name already exists
- `400 Bad Request` for invalid name

### POST /guests

Creates a guest profile, joins one game immediately, and issues session cookie.

Request:
```json
{
	"name": "Guest-Blue",
	"gameId": "game_123"
}
```

Response `201 Created`:
```json
{
	"player": {
		"id": "ply_guest987",
		"name": "Guest-Blue",
		"kind": "guest"
	},
	"joinedGameId": "game_123"
}
```

Errors:
- `404 Not Found` if the provided game does not exist
- `409 Conflict` if game cannot be joined in current state

### GET /players/me

Returns current authenticated player profile.

Response `200 OK`:
```json
{
	"player": {
		"id": "ply_abc123",
		"name": "alice",
		"kind": "user",
		"state": "active"
	}
}
```

### GET /players/{playerId}

Returns player profile.

Visibility rule:
- Allowed only when requester is that player or currently shares a game.

### DELETE /users/me

Deactivates the current user account.

Responses:
- `204 No Content`
- `409 Conflict` when account type mismatch occurs unexpectedly

### Credentials

Only user accounts manage password credentials.

### GET /credentials

Lists credentials owned by authenticated user.

Response `200 OK`:
```json
[
	{
		"id": "cred_pwd_1",
		"type": "password",
		"expiresAt": null
	},
	{
		"id": "cred_sess_2",
		"type": "sessionToken",
		"expiresAt": "2026-03-28T10:15:00Z"
	}
]
```

### POST /credentials/password

Creates or rotates password credential for current user.

Request:
```json
{
	"password": "new secure password"
}
```

Responses:
- `201 Created`
- `401 Unauthorized`
- `403 Forbidden` for guest account

### DELETE /credentials/{credentialId}

Revokes one credential owned by current user.

Responses:
- `204 No Content`
- `404 Not Found` when credential does not belong to caller

### DELETE /credentials/session

Revokes all session-token credentials for current user.

Responses:
- `204 No Content`

### Games

All games are private in this API design.

### POST /games

Creates a game with the caller as admin and participant.

Request:
```json
{
	"name": "Office April Round",
	"description": "Spring event",
	"endTime": "2026-04-15T18:00:00Z"
}
```

Response `201 Created`:
```json
{
	"game": {
		"id": "game_123",
		"name": "Office April Round",
		"description": "Spring event",
		"state": "pending",
		"adminPlayerId": "ply_abc123",
		"startTime": null,
		"endTime": "2026-04-15T18:00:00Z"
	}
}
```

### GET /games/{gameId}

Returns game metadata for participants only.

### PATCH /games/{gameId}

Updates mutable metadata (`name`, `description`, optional `endTime`).

Auth:
- Game admin only

### DELETE /games/{gameId}

Deletes game.

Auth:
- Game admin only

### GET /players/me/games

Lists games joined by current player.

Response `200 OK`:
```json
{
	"games": ["game_123", "game_456"]
}
```

### POST /games/{gameId}/join

Adds current player as participant.

Guest-specific rule:
- Guests cannot use this endpoint.
- Guests must be created with their target game via `POST /guests`.

Responses:
- `204 No Content`
- `403 Forbidden` when caller is a guest
- `409 Conflict` for invalid game state (e.g., already running)

### POST /games/{gameId}/leave

Removes current player from game.

Behavior:
- In pending state, admin handover follows domain rules.
- In running/ended state, operation is invalid.
- For guest players, leave is equivalent to deleting the guest identity:
	- remove guest from game
	- revoke guest session credentials
	- delete/deactivate guest account record

### POST /games/{gameId}/start

Starts game and initializes murder chain.

Request:
```json
{
	"endTime": "2026-04-15T18:00:00Z"
}
```

Auth:
- Game admin only

Responses:
- `204 No Content`
- `409 Conflict` when not enough participants
- `409 Conflict` for invalid game state

### POST /games/{gameId}/end

Ends game immediately.

Auth:
- Game admin only

### PATCH /games/{gameId}/end

Changes planned end time.

Request:
```json
{
	"endTime": "2026-04-20T12:00:00Z"
}
```

Auth:
- Game admin only

### Gameplay

### GET /games/{gameId}/victim

Returns current caller victim in a running game.

Response `200 OK`:
```json
{
	"victimPlayerId": "ply_target007"
}
```

Responses:
- `409 Conflict` when game is not running
- `409 Conflict` when caller is dead or no victims remain

### POST /games/{gameId}/kills

Submits a kill action.

Request:
```json
{
	"victimPlayerId": "ply_target007"
}
```

Response `200 OK`:
```json
{
	"nextVictimPlayerId": "ply_next999",
	"gameEnded": false
}
```

If no next victim exists:
```json
{
	"nextVictimPlayerId": null,
	"gameEnded": true
}
```

Responses:
- `409 Conflict` for incorrect victim
- `409 Conflict` when caller is dead/not participating
- `409 Conflict` when game is not running

### GET /games/{gameId}/leaderboard

Returns current kills per participant.

Response `200 OK`:
```json
{
	"entries": [
		{ "playerId": "ply_abc123", "kills": 2 },
		{ "playerId": "ply_target007", "kills": 1 }
	]
}
```

### GET /games/{gameId}/participants

Returns participant list.

Response `200 OK`:
```json
{
	"participants": [
		{ "id": "ply_abc123", "name": "alice", "kind": "user" },
		{ "id": "ply_guest987", "name": "Guest-Blue", "kind": "guest" }
	]
}
```

## Error Model (Problem Details)

All non-2xx responses should be returned as Problem Details.

Example:
```json
{
	"type": "https://api.murder/errors/game-not-found",
	"title": "Game not found",
	"status": 404,
	"detail": "Game 'game_123' does not exist.",
	"instance": "/games/game_123"
}
```

### Exception to HTTP Mapping

| Exception / Condition | HTTP | Problem type |
|---|---:|---|
| Missing or invalid session | 401 | `/errors/unauthorized` |
| Authenticated but not allowed | 403 | `/errors/forbidden` |
| `GameNotFoundException` | 404 | `/errors/game-not-found` |
| `UnexpectedGameStateException` | 409 | `/errors/invalid-game-state` |
| `NotEnoughParticipantsException` | 409 | `/errors/not-enough-participants` |
| `PlayerNotParticipating` | 403 | `/errors/not-participant` |
| `PlayerDeadException` | 409 | `/errors/player-dead` |
| `IncorrectVictimException` | 409 | `/errors/incorrect-victim` |
| `NoMoreVictimsException` | 409 | `/errors/no-more-victims` |
| Username already exists | 409 | `/errors/name-conflict` |
| `UserGuestMismatchException` | 409 | `/errors/identity-kind-mismatch` |
| Unsupported auth/credential method | 400 | `/errors/unsupported-method` |
| Validation failure | 400 | `/errors/validation` |

## Domain Rule Notes (Implementation Guards)

- `Join` and `Leave` are pending-state operations only.
- `Start` requires at least 2 participants.
- `Victim` and `Kill` are running-state operations only.
- `Leaderboard` is available only after start.
- `Kill` must target exactly the current assigned victim.
- Guest accounts are created with exactly one game and cannot join later.
- Guest leave and guest self-deletion are equivalent operations.
- Admin-only operations must validate current player equals game admin.

## Service Traceability

| API Endpoint / Group | Service method(s) |
|---|---|
| `POST /auth/login` | `AuthenticationService.Authenticate<PasswordMethodKey>()`, `AuthenticationService.AddMethod<SessionTokenMethodKey>()` |
| `POST /auth/logout` | `AuthenticationService.RevokeCredential()` |
| `POST /users` | `IdentityService.CreateUser()` |
| `POST /guests` | `IdentityService.CreateGuest()`, `GameService.JoinGame()` |
| `GET /players/me`, `GET /players/{playerId}` | `IdentityService.GetIdentity()` |
| `DELETE /users/me` | `IdentityService.DeactivateUser()` |
| `DELETE /players/me` | `IdentityService.DeactivateUser()` (user), guest deletion workflow + game leave |
| `GET /credentials` | `ICredentialRepository.FindAll<TMethod>()`, `ICredentialRepository.FindById<TMethod>()` |
| `POST /credentials/password` | `AuthenticationService.AddMethod<PasswordMethodKey>()` |
| `DELETE /credentials/{credentialId}` | `AuthenticationService.RevokeCredential()` |
| `DELETE /credentials/session` | `AuthenticationService.RemoveMethod<SessionTokenMethodKey>()` |
| `POST /games` | `GameService.CreateGame()` |
| `GET /players/me/games` | `GameService.ListJoinedGames()` |
| `GET /games/{gameId}` | `GameService.GetGame()` |
| `POST /games/{gameId}/join` | `GameService.JoinGame()` |
| `POST /games/{gameId}/leave` | `GameService.LeaveGame()` |
| `PATCH /games/{gameId}` | `GameService.RenameGame()`, `GameService.DescribeGame()`, `GameService.ChangeEnd()` |
| `DELETE /games/{gameId}` | `GameService.DeleteGame()` |
| `POST /games/{gameId}/start` | `GameService.StartGame()` |
| `POST /games/{gameId}/end` | `GameService.EndGame()` |
| `PATCH /games/{gameId}/end` | `GameService.ChangeEnd()` |
| `GET /games/{gameId}/victim` | `GameService.Victim()` |
| `POST /games/{gameId}/kills` | `GameService.Kill()` |
| `GET /games/{gameId}/leaderboard` | `GameService.Leaderboard()` |
| `GET /games/{gameId}/participants` | `GameService.Participants()` |

## Notes About Existing Endpoints

Current implemented endpoints (`/login`, `/user`, `/self`, `/credential`) are a prototype.
During implementation, align them to this document even if route names and DTOs change.

