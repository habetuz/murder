import type { ServerResponse, IncomingMessage } from 'http';
import { getHealth } from './logic/health.js';
import { getRoot, postRoot } from './logic/root.js';
import { getGame } from './logic/game.js';

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
    endpoint: '/',
    method: 'POST',
    handler: postRoot,
  },
  {
    endpoint: '/game',
    method: 'GET',
    handler: getGame,
  }
] as Route[];
