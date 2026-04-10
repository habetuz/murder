// API response and request types

export interface PlayerRef {
  id: string
  name: string | null
  kind: 'user' | 'guest'
  state?: 'active' | 'deactivated'
}

export interface SessionInfo {
  expiresAt: string | null
}

export interface GameDto {
  id: string
  name: string
  description: string | null
  state: 'pending' | 'running' | 'ended'
  adminPlayerId: string
  startTime: string | null
  endTime: string | null
}

export interface ParticipantDto {
  id: string
  name: string
  username: string | null
  kind: 'user' | 'guest'
}

export interface LeaderboardEntry {
  playerId: string
  name: string
  kills: number
}

export interface CredentialDto {
  id: string
  type: 'password' | 'sessionToken'
  expiresAt: string | null
}

// Requests
export interface LoginRequest {
  type: 'password'
  username: string
  password: string
}

export interface CreateUserRequest {
  name: string
}

export interface CreateGuestRequest {
  name: string
  gameId: string
}

export interface CreateGameRequest {
  name: string
  displayName: string
  description?: string
  endTime?: string
}

export interface PatchGameRequest {
  name?: string
  description?: string
  endTime?: string
}

export interface StartGameRequest {
  endTime?: string
}

export interface ChangeEndRequest {
  endTime: string
}

export interface KillRequest {
  victimPlayerId: string
}

export interface JoinGameRequest {
  name: string
}

// Responses
export interface SessionResponse {
  player: PlayerRef
  session: SessionInfo
}

export interface CreateUserResponse {
  player: PlayerRef
}

export interface CreateGuestResponse {
  player: PlayerRef
  joinedGameId: string
}

export interface GameResponse {
  game: GameDto
}

export interface MyGamesResponse {
  games: string[]
}

export interface VictimResponse {
  victimPlayerId: string
  victimName: string
}

export interface KillResponse {
  status: 'pending'
}

export interface KillRespondRequest {
  accepted: boolean
}

export interface KillRespondResponse {
  result: 'confirmed' | 'denied'
}

export interface LeaderboardResponse {
  entries: LeaderboardEntry[]
}

export interface ParticipantsResponse {
  participants: ParticipantDto[]
}

export interface RestoreTokenResponse {
  token: string
}

export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  instance?: string
}

// Watch (SSE) types

export interface WatchPlayerState {
  victimPlayerId: string | null
  victimName: string | null
  alive: boolean
  pendingKill: boolean
  pendingKillSent: boolean
}

export interface WatchPayload {
  game: GameDto
  participants: ParticipantDto[]
  leaderboard: LeaderboardEntry[] | null
  me: WatchPlayerState
}
