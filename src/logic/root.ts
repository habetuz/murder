import type { IncomingMessage, ServerResponse } from 'http';
import * as fs from 'fs/promises';
import path from 'path';

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
