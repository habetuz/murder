import type { IncomingMessage, ServerResponse } from 'http';
import { State } from '../global.js';
import * as fs from 'fs/promises';
import * as path from 'path';



export async function getAdminPage(_: IncomingMessage, res: ServerResponse) {
  res.writeHead(200, { 'content-type': 'text/html' });
  const page = await fs.readFile(
    path.join(import.meta.dirname, '..', '..', 'site', 'adminPage.html')
  );
  res.end(page);
}
