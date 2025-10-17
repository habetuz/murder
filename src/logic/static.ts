import type { IncomingMessage, ServerResponse } from 'http';
import * as fs from 'fs/promises';
import path from 'path';

export async function getStaticFile(req: IncomingMessage, res: ServerResponse) {
  const url = new URL(req.url ?? '/', `http://${req.headers.host}`);
  try {
    const file = await fs.readFile(
      path.join(import.meta.dirname, '..', '..', url.pathname)
    );

    res.statusCode = 200;
    res.end(file);
  } catch (error) {
    console.error('Error serving static file:', error);
    res.statusCode = 404;
    res.end();
  }
}
