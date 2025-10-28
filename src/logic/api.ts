import type { IncomingMessage, ServerResponse } from 'http';
import { State } from '../global.js';
import { parseRequestBody, shuffle } from '../utils.js';

export function getPlayers(_: IncomingMessage, res: ServerResponse) {
  res.writeHead(200, { 'content-type': 'application/json' });
  res.end(JSON.stringify(State.players));
}

export async function postRegisterPlayer(
  req: IncomingMessage,
  res: ServerResponse
): Promise<void> {
  if (req.headers['content-type'] !== 'application/x-www-form-urlencoded') {
    res.writeHead(415, {
      'content-type': 'text/plain',
    });
    res.end('Only application/x-www-form-urlencoded is supported');
    return;
  }

  const body = await parseRequestBody(req);
  const formData = new URLSearchParams(body);
  const name = formData.get('name')?.trim();

  if (!name) {
    res.writeHead(400, {
      'content-type': 'text/plain',
    });
    res.end('Name is required');
    return;
  }

  if (State.players.includes(name)) {
    res.writeHead(409, {
      'content-type': 'text/plain',
    });
    res.end('Name is already taken');
    return;
  }

  console.info(`Player joined with name: ${name}`);
  State.players.push(name);

  res.writeHead(302, {
    Location: `/game?player=${encodeURIComponent(name)}`,
  });
  res.end();
}

export function postStartGame(_: IncomingMessage, res: ServerResponse) {
  const shuffledPlayers = shuffle([...State.players]);
  State.assignments = new Map<string, string>();
  for (let i = 0; i < shuffledPlayers.length; i++) {
    const player = shuffledPlayers[i] as string;
    const target = shuffledPlayers[(i + 1) % shuffledPlayers.length] as string;
    State.assignments.set(player, target);
  }
  
  console.info('Game started with assignments:', State.assignments);
  
  res.writeHead(200);
  res.end();
}
