# HR Analytics Platform

Enterprise-grade HR analytics platform built with Angular 21, NgRx, and real-time WebSocket support. Provides workforce analytics, performance management, and hiring forecasts.

## Stack

- **Frontend**: Angular 21, NgRx (state management), Tailwind CSS, ECharts
- **Language**: TypeScript
- **Build tool**: Angular CLI / Vite
- **Testing**: Vitest

## How to run

```bash
cd frontend && npm start
```

The app serves on port 5000 (`http://0.0.0.0:5000`). The workflow "Start application" handles this automatically.

## Project structure

```
frontend/        Angular application
docs/            Full project documentation (API, architecture, user guide, etc.)
.github/         CI/CD workflow configs
Dockerfile       Container definition
```

## Notes

- The app shows a "Disconnected" WebSocket status indicator — it expects a backend WebSocket server on `ws://localhost:8080/` that is not included in this repo (frontend-only import).
- Dependencies must be installed via `cd frontend && npm install` before the first run.

## User preferences

<!-- Add any user preferences here -->
