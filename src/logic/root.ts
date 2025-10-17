import type { IncomingMessage, ServerResponse } from 'http';
import * as fs from 'fs/promises';
import path from 'path';
import { parseRequestBody } from '../utils.js';
import { State } from '../global.js';

export async function getRoot(
  _: IncomingMessage,
  res: ServerResponse
): Promise<void> {
  const buffer = await fs.readFile(
    path.join(import.meta.dirname, '..', '..', 'site', 'root.html')
  );

  const content = buffer.toString();
  res.writeHead(200, {
    'content-type': 'text/html',
  });
  res.end(content);
}

export async function postRoot(
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
