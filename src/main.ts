import http from 'http';
import handlers from './handlers.js';
import { getStaticFile } from './logic/static.js';
import QRCode from 'qrcode';
import * as fs from 'fs/promises';

const PORT = 8080;

const server = http.createServer((req, res) => {
  try {
    console.info(`Received ${req.method} request for ${req.url}`);

    const url = new URL(req.url ?? '/', `http://${req.headers.host}`);

    if (url.pathname.startsWith('/static')) {
      void getStaticFile(req, res);
      return;
    }

    const handler = handlers.find(
      (handler) =>
        handler.endpoint === url.pathname && handler.method === req.method
    );

    if (handler === undefined) {
      const endpointExists = handlers.some(
        (handler) => handler.endpoint == url.pathname
      );

      if (endpointExists) {
        res.statusCode = 405;
      } else {
        res.statusCode = 404;
      }

      res.end();
    } else {
      void handler.handler(req, res);
    }
  } catch (error) {
    console.error('Error handling request:', error);
    res.statusCode = 500;
    res.end('Internal Server Error');
  }
});

const url = process.env['URL'];
if (!url) {
  throw new Error('URL environment variable is not set');
}

fs.mkdir('./static', { recursive: true });

await QRCode.toFile('./static/qrcode.png', url);

server.listen(PORT, () => {
  console.info(`Server is running on port ${PORT}`);
});
