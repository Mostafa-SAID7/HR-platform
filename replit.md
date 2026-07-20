# HR Analytics Platform

Enterprise-grade HR analytics platform built with Angular 21, NgRx, and Tailwind CSS v4.

## Stack

- **Frontend**: Angular 21 (standalone components), NgRx state management, Tailwind CSS v4, RxJS
- **Build tool**: Angular CLI with Vite-based dev server
- **Language**: TypeScript (strict mode)

## Project Structure

```
frontend/       # Angular application (all source code lives here)
  src/
    app/
      core/       # Services, guards, interceptors
      features/   # Feature modules (dashboard, employees, performance, etc.)
      shared/     # Reusable components, pipes, directives, widgets
      layouts/    # App shell layouts
    styles.css    # Global styles (includes Tailwind v4 import)
  angular.json    # Angular CLI config (dev server: 0.0.0.0:5000)
docs/           # Documentation (user guide, admin guide, technical docs)
```

## How to Run

The app runs via the **"Start application"** workflow, which executes:
```bash
cd frontend && npm start
```

This starts `ng serve` on port 5000 (bound to `0.0.0.0` for Replit preview access).

## Development

```bash
cd frontend

# Start dev server
npm start

# Build for production
npm run build

# Run unit tests
npm test

# Lint
npm run lint
```

## Key Notes

- **No backend** — this is a frontend-only project. API calls to `api.hrplatform.com` will fail; all data is mocked/stubbed.
- Auth guard is set to `return true` by default (bypass for development).
- Tailwind v4 uses `@import "tailwindcss"` in `styles.css` (not `@tailwind` directives).
- The `Logger` utility uses `window.location.hostname` to detect production (replaces the old `ng.probe` call which was removed in Angular Ivy).

## User Preferences

<!-- Add any user-specific preferences here -->
