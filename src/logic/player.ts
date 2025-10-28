import type { IncomingMessage, ServerResponse } from 'http';
import { State } from '../global.js';
import * as fs from 'fs/promises';
import path from 'path';

export async function getPlayerPage(req: IncomingMessage, res: ServerResponse) {
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

export function getPlayerStatus(req: IncomingMessage, res: ServerResponse) {
  if (!State.assignments) {
    res.writeHead(200, { 'content-type': 'text/html' });
    res.end('<p>The game has not started yet.</p>');
  } else {
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

    const target = State.assignments.get(player);
    if (!target) {
      console.log('No assignment found for player:', player);
      res.writeHead(500)
      res.end('Internal server error');
      return;
    }
    
    res.writeHead(200, { 'content-type': 'text/html' });
    res.end(`
      <p>Your target is: <sub>${target}</sub></p>
      <p>Write down your target on a piece of paper. Do not share this information with other players!</p>
      <p>Good luck!</p>
    `);
  }
}
