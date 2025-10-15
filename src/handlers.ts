import type { ServerResponse, IncomingMessage } from 'http';
import { handleHealth } from './logic/health.js';

type Route = {
  endpoint: string;
  method: string;
  handler: (req: IncomingMessage, res: ServerResponse) => Promise<void>;
};

export default [
  {
    endpoint: '/health',
    method: 'GET',
    handler: handleHealth,
  },
] as Route[];
