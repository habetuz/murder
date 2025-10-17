import type { IncomingMessage, ServerResponse } from 'http';

export function getHealth(
  _: IncomingMessage,
  res: ServerResponse
) {
  res.statusCode = 200;
  res.end()
}
