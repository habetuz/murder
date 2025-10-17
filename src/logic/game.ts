import type { IncomingMessage, ServerResponse } from 'http';
import { State } from '../global.js';
import * as fs from 'fs/promises';
import path from 'path';

export async function getGame(req: IncomingMessage, res: ServerResponse) {
  if (!req.url) {
    res.writeHead(404, { 'content-type': 'text/plain' });
    res.end('Player not found');
    return;
  }

  const url = new URL(req.url, `http://${req.headers.host}`);
  const player = url.searchParams.get('player');
  if (!player) {
    res.writeHead(404, { 'content-type': 'text/plain' });
    res.end('Player not found');
    return;
  }

  if (!State.players.includes(player)) {
    res.writeHead(404, { 'content-type': 'text/plain' });
    res.end('Player not found');
    return;
  }

  res.writeHead(200, { 'content-type': 'text/html' });
  const file = await fs.readFile(
    path.join(import.meta.dirname, '..', '..', 'site', 'playerPage.html')
  );

  res.end(file);
}
