import type { ServerResponse, IncomingMessage } from 'http';
import { getHealth } from './logic/health.js';
import { getRoot } from './logic/root.js';
import { getPlayerPage, getPlayerStatus } from './logic/player.js';
import { getAdminPage } from './logic/admin.js';
import { getPlayers, postRegisterPlayer, postStartGame } from './logic/api.js';

type Route = {
  endpoint: string;
  method: string;
  handler: (req: IncomingMessage, res: ServerResponse) => Promise<void>;
};

export default [
  {
    endpoint: '/health',
    method: 'GET',
    handler: getHealth,
  },
  {
    endpoint: '/',
    method: 'GET',
    handler: getRoot,
  },
  {
    endpoint: '/game',
    method: 'GET',
    handler: getPlayerPage,
  },
  {
    endpoint: '/status',
    method: 'GET',
    handler: getPlayerStatus,
  },
  {
    endpoint: '/admin',
    method: 'GET',
    handler: getAdminPage,
  },
  {
    endpoint: '/api/players',
    method: 'GET',
    handler: getPlayers,
  },
  {
    endpoint: '/api/start-game',
    method: 'POST',
    handler: postStartGame,
  },
  {
    endpoint: '/api/players',
    method: 'POST',
    handler: postRegisterPlayer,
  },
] as Route[];
