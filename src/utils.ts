import type { IncomingMessage } from 'http';

export async function parseRequestBody(req: IncomingMessage): Promise<string> {
  return new Promise((resolve, reject) => {
    let body = '';
    req.on('data', (chunk) => {
      body += chunk.toString();
    });
    req.on('end', () => {
      resolve(body);
    });
    req.on('error', (err) => {
      reject(err);
    });
  });
}

// Fisher–Yates Shuffle from https://bost.ocks.org/mike/shuffle/
export function shuffle<T>(array: Array<T>) {
  var m = array.length, t, i;

  // While there remain elements to shuffle…
  while (m) {

    // Pick a remaining element…
    i = Math.floor(Math.random() * m--);

    // And swap it with the current element.
    t = array[m] as T;
    array[m] = array[i] as T;
    array[i] = t;
  }

  return array;
}
