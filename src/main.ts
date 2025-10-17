import http from 'http';
import handlers from './handlers.js';

const PORT = 8080;

const server = http.createServer((req, res) => {
  try {
    const url = new URL(req.url ?? '/', `http://${req.headers.host}`);
    
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

server.listen(PORT, () => {
  console.info(`Server is running on port ${PORT}`);
});
