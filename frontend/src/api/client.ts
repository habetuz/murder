import { ApiError, type CredentialDto, type GameDto, type LeaderboardEntry, type ParticipantDto, type PlayerProfile, type SessionPayload } from '../types/api';

const API_BASE = (import.meta.env.VITE_API_BASE_URL ?? '/api').replace(/\/$/, '');

async function parseBody(response: Response): Promise<unknown> {
  const contentType = response.headers.get('content-type') ?? '';
  if (contentType.includes('application/json') || contentType.includes('application/problem+json')) {
    return response.json();
  }

  return null;
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const normalizedPath = path.startsWith('/') ? path : `/${path}`;
  const response = await fetch(`${API_BASE}${normalizedPath}`, {
    credentials: 'include',
    headers: {
      'Content-Type': 'application/json',
      ...(init?.headers ?? {}),
    },
    ...init,
  });

  const body = await parseBody(response);

  if (!response.ok) {
    const problem = typeof body === 'object' && body !== null ? body : undefined;
    const message = (problem as { detail?: string } | undefined)?.detail ?? response.statusText;
    throw new ApiError(response.status, message, problem as never);
  }

  return body as T;
}

export const api = {
  login(payload: { type: 'password'; username: string; password: string }) {
    return request<void>('/auth/login', {
      method: 'POST',
      body: JSON.stringify(payload),
    });
  },

  logout() {
    return request<void>('/auth/logout', { method: 'POST' });
  },

  session() {
    return request<SessionPayload>('/auth/session');
  },

  createUser(payload: { name: string }) {
    return request<{ player: PlayerProfile }>('/users', {
      method: 'POST',
      body: JSON.stringify(payload),
    });
  },

  createGuest(payload: { name: string; gameId: string }) {
    return request<{ player: PlayerProfile; joinedGameId: string }>('/guests', {
      method: 'POST',
      body: JSON.stringify(payload),
    });
  },

  getMe() {
    return request<{ player: PlayerProfile }>('/players/me');
  },

  listMyGames() {
    return request<{ games: string[] }>('/players/me/games');
  },

  createGame(payload: { name: string; description?: string; endTime?: string }) {
    return request<{ game: GameDto }>('/games', {
      method: 'POST',
      body: JSON.stringify(payload),
    });
  },

  getGame(gameId: string) {
    return request<{ game: GameDto }>(`/games/${encodeURIComponent(gameId)}`);
  },

  patchGame(gameId: string, payload: { name?: string; description?: string; endTime?: string }) {
    return request<void>(`/games/${encodeURIComponent(gameId)}`, {
      method: 'PATCH',
      body: JSON.stringify(payload),
    });
  },

  joinGame(gameId: string) {
    return request<void>(`/games/${encodeURIComponent(gameId)}/join`, { method: 'POST' });
  },

  leaveGame(gameId: string) {
    return request<void>(`/games/${encodeURIComponent(gameId)}/leave`, { method: 'POST' });
  },

  startGame(gameId: string, payload: { endTime?: string }) {
    return request<void>(`/games/${encodeURIComponent(gameId)}/start`, {
      method: 'POST',
      body: JSON.stringify(payload),
    });
  },

  endGame(gameId: string) {
    return request<void>(`/games/${encodeURIComponent(gameId)}/end`, { method: 'POST' });
  },

  changeEnd(gameId: string, endTime: string) {
    return request<void>(`/games/${encodeURIComponent(gameId)}/end`, {
      method: 'PATCH',
      body: JSON.stringify({ endTime }),
    });
  },

  participants(gameId: string) {
    return request<{ participants: ParticipantDto[] }>(`/games/${encodeURIComponent(gameId)}/participants`);
  },

  victim(gameId: string) {
    return request<{ victimPlayerId: string }>(`/games/${encodeURIComponent(gameId)}/victim`);
  },

  kill(gameId: string, victimPlayerId: string) {
    return request<{ nextVictimPlayerId: string | null; gameEnded: boolean }>(`/games/${encodeURIComponent(gameId)}/kills`, {
      method: 'POST',
      body: JSON.stringify({ victimPlayerId }),
    });
  },

  leaderboard(gameId: string) {
    return request<{ entries: LeaderboardEntry[] }>(`/games/${encodeURIComponent(gameId)}/leaderboard`);
  },

  getCredentials() {
    return request<CredentialDto[]>('/credentials');
  },

  setPassword(password: string) {
    return request<{ id: string; type: string }>('/credentials/password', {
      method: 'POST',
      body: JSON.stringify({ password }),
    });
  },

  revokeCredential(credentialId: string) {
    return request<void>(`/credentials/${encodeURIComponent(credentialId)}`, { method: 'DELETE' });
  },

  revokeSessions() {
    return request<void>('/credentials/session', { method: 'DELETE' });
  },
};
