# HR Analytics Platform

Enterprise-grade HR analytics platform built with Angular 21, NgRx, and real-time WebSocket support. Provides workforce analytics, performance management, hiring forecasts, and attendance tracking.

## Stack

- **Framework**: Angular 21 (standalone components)
- **State management**: NgRx (store, effects, entity)
- **Styling**: Tailwind CSS
- **Build tool**: Angular CLI / Vite
- **Language**: TypeScript

## Project structure

```
frontend/   Angular application (all source code lives here)
docs/       User, admin, and technical documentation
```

## How to run

The workflow `Start application` runs `cd frontend && npm start`, which starts the Angular dev server on port 5000.

```bash
cd frontend && npm start
```

The app is available at port 5000 in the Replit preview.

## Notes

- `caniuse-lite` is pinned to `1.0.30001579` — newer versions (≥ 1.0.30001580) removed the `dist/unpacker/agents` file that `browserslist` v4.28.6 requires. Do not upgrade `caniuse-lite` without also upgrading `browserslist` to a compatible version.
- The app shows a "Disconnected" WebSocket indicator in the header — this is expected in the dev environment since no backend WebSocket server is running.

## User preferences

<!-- Add user preferences here -->
