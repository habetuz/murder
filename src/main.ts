import http from 'http';
import handlers from './handlers.js';

const PORT = 8080;

const server = http.createServer((req, res) => {
  const handler = handlers.find(
    (handler) => handler.endpoint === req.url && handler.method === req.method
  );

  console.info(`New request to ${req.url}`);

  if (handler === undefined) {
    const endpointExists = handlers.some(
      (handler) => handler.endpoint == req.url
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
});

server.listen(PORT, () => {
  console.info(`Server is running on port ${PORT}`);
});
