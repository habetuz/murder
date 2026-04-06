export type PlayerKind = 'user' | 'guest';

export interface PlayerRef {
  id: string;
  name: string;
  kind: PlayerKind;
}

export interface PlayerProfile extends PlayerRef {
  state: string | null;
}

export interface SessionPayload {
  player: PlayerRef;
  session: {
    expiresAt: string | null;
  };
}

export interface CredentialDto {
  id: string;
  type: string;
  expiresAt: string | null;
}

export type GameState = 'pending' | 'running' | 'ended';

export interface GameDto {
  id: string;
  name: string;
  description: string | null;
  state: GameState;
  adminPlayerId: string | null;
  startTime: string | null;
  endTime: string | null;
}

export interface ParticipantDto {
  id: string;
  name: string;
  kind: PlayerKind;
}

export interface LeaderboardEntry {
  playerId: string;
  kills: number;
}

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
}

export class ApiError extends Error {
  status: number;
  problem?: ProblemDetails;

  constructor(status: number, message: string, problem?: ProblemDetails) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.problem = problem;
  }
}
