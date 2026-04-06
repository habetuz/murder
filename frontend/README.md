# Murder Frontend

Vue 3 + PrimeVue frontend for the Murder game platform.

## Features

- Start screen for anonymous and authenticated users
- Game screen with pending/running/ended states
- Victim reveal via intentional hold-to-reveal interaction
- User settings screen for credential management
- Mobile-first layout
- Installable PWA via `vite-plugin-pwa`

## Run

```bash
bun install
cp .env.example .env
bun dev
```

Frontend runs on `http://localhost:5173` by default.

Backend should run on `http://localhost:8080`.

## Build

```bash
bun run build
bun run preview
```

## Environment

- `VITE_API_BASE_URL`: Base URL of the WebAPI (default: `/api`)
