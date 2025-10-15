import type { IncomingMessage, ServerResponse } from 'http';

export function handleHealth(
  _: IncomingMessage,
  res: ServerResponse
) {
  res.statusCode = 200;
  res.end()
}
