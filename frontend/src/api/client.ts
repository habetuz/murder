import type {
  ProblemDetails,
  LoginRequest,
  CreateUserRequest,
  CreateGuestRequest,
  CreateGameRequest,
  PatchGameRequest,
  StartGameRequest,
  ChangeEndRequest,
  KillRequest,
  JoinGameRequest,
  SessionResponse,
  CreateUserResponse,
  CreateGuestResponse,
  GameResponse,
  MyGamesResponse,
  VictimResponse,
  KillResponse,
  LeaderboardResponse,
  ParticipantsResponse,
  CredentialDto,
  PlayerRef,
} from '../types/api'

const BASE = import.meta.env.VITE_API_BASE_URL ?? '/api'

type UnauthorizedHandler = (message: string) => void
let onUnauthorized: UnauthorizedHandler | null = null

export function registerUnauthorizedHandler(handler: UnauthorizedHandler) {
  onUnauthorized = handler
}

export class ApiError extends Error {
  readonly status: number
  readonly problem: ProblemDetails

  constructor(status: number, problem: ProblemDetails) {
    super(problem.title ?? `HTTP ${status}`)
    this.status = status
    this.problem = problem
  }

  get type() {
    return this.problem.type ?? ''
  }

  is(problemType: string) {
    return this.type.endsWith(problemType)
  }
}

async function request<T>(path: string, init: RequestInit = {}, opts: { skipAuthHandler?: boolean } = {}): Promise<T> {
  const res = await fetch(`${BASE}${path}`, {
    ...init,
    credentials: 'include',
    headers: {
      'Content-Type': 'application/json',
      ...init.headers,
    },
  })

  const contentType = res.headers.get('Content-Type') ?? ''
  const contentLength = res.headers.get('Content-Length')
  const hasBody = contentLength !== '0' && res.status !== 204

  if (!res.ok) {
    let problem: ProblemDetails = { status: res.status, title: res.statusText }
    if (hasBody && (contentType.includes('problem+json') || contentType.includes('application/json'))) {
      try {
        problem = await res.json()
      } catch {
        // use default
      }
    }
    if (res.status === 401 && !opts.skipAuthHandler && onUnauthorized) {
      onUnauthorized(problem.detail ?? 'Your session has expired. Please log in again.')
    }
    throw new ApiError(res.status, problem)
  }

  if (!hasBody || !contentType.includes('application/json')) return undefined as T

  return res.json() as Promise<T>
}

export const api = {
  // Auth — skip global handler; callers handle 401 locally
  login: (body: LoginRequest) =>
    request<void>('/auth/login', { method: 'POST', body: JSON.stringify(body) }, { skipAuthHandler: true }),

  logout: () =>
    request<void>('/auth/logout', { method: 'POST' }, { skipAuthHandler: true }),

  getSession: () =>
    request<SessionResponse>('/auth/session', {}, { skipAuthHandler: true }),

  // Identity
  createUser: (body: CreateUserRequest) =>
    request<CreateUserResponse>('/users', { method: 'POST', body: JSON.stringify(body) }),

  createGuest: (body: CreateGuestRequest) =>
    request<CreateGuestResponse>('/guests', { method: 'POST', body: JSON.stringify(body) }),

  getMe: () =>
    request<{ player: PlayerRef }>('/players/me'),

  getPlayer: (playerId: string) =>
    request<{ player: PlayerRef }>(`/players/${encodeURIComponent(playerId)}`),

  deleteMe: () =>
    request<void>('/users/me', { method: 'DELETE' }),

  // Credentials
  getCredentials: () =>
    request<CredentialDto[]>('/credentials'),

  createPassword: (password: string) =>
    request<void>('/credentials/password', { method: 'POST', body: JSON.stringify({ password }) }),

  deleteCredential: (id: string) =>
    request<void>(`/credentials/${encodeURIComponent(id)}`, { method: 'DELETE' }),

  revokeAllSessions: () =>
    request<void>('/credentials/session', { method: 'DELETE' }),

  // Games
  createGame: (body: CreateGameRequest) =>
    request<GameResponse>('/games', { method: 'POST', body: JSON.stringify(body) }),

  getGame: (gameId: string) =>
    request<GameResponse>(`/games/${encodeURIComponent(gameId)}`),

  patchGame: (gameId: string, body: PatchGameRequest) =>
    request<void>(`/games/${encodeURIComponent(gameId)}`, { method: 'PATCH', body: JSON.stringify(body) }),

  deleteGame: (gameId: string) =>
    request<void>(`/games/${encodeURIComponent(gameId)}`, { method: 'DELETE' }),

  getMyGames: () =>
    request<MyGamesResponse>('/players/me/games'),

  joinGame: (gameId: string, body: JoinGameRequest) =>
    request<void>(`/games/${encodeURIComponent(gameId)}/join`, { method: 'POST', body: JSON.stringify(body) }),

  leaveGame: (gameId: string) =>
    request<void>(`/games/${encodeURIComponent(gameId)}/leave`, { method: 'POST' }),

  startGame: (gameId: string, body?: StartGameRequest) =>
    request<void>(`/games/${encodeURIComponent(gameId)}/start`, { method: 'POST', body: JSON.stringify(body ?? {}) }),

  endGame: (gameId: string) =>
    request<void>(`/games/${encodeURIComponent(gameId)}/end`, { method: 'POST' }),

  changeEnd: (gameId: string, body: ChangeEndRequest) =>
    request<void>(`/games/${encodeURIComponent(gameId)}/end`, { method: 'PATCH', body: JSON.stringify(body) }),

  // Gameplay
  getVictim: (gameId: string) =>
    request<VictimResponse>(`/games/${encodeURIComponent(gameId)}/victim`),

  submitKill: (gameId: string, body: KillRequest) =>
    request<KillResponse>(`/games/${encodeURIComponent(gameId)}/kills`, { method: 'POST', body: JSON.stringify(body) }),

  getLeaderboard: (gameId: string) =>
    request<LeaderboardResponse>(`/games/${encodeURIComponent(gameId)}/leaderboard`),

  getParticipants: (gameId: string) =>
    request<ParticipantsResponse>(`/games/${encodeURIComponent(gameId)}/participants`),
}
