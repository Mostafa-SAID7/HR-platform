# HR Analytics Platform - Setup Guide

## Project Configuration

This project is configured with the following setup:

### Build System
- **Vite**: Modern build tool with fast development server and optimized production builds
- **esbuild**: JavaScript bundler and minifier for optimized output
- **Angular 21**: Latest Angular framework with standalone components

### TypeScript Configuration
- **Strict Mode**: Enabled for type safety
- **Path Aliases**: Configured for cleaner imports
  - `@app/*` → `src/app/*`
  - `@components/*` → `src/app/components/*`
  - `@services/*` → `src/app/services/*`
  - `@models/*` → `src/app/models/*`
  - `@store/*` → `src/app/store/*`
  - `@utils/*` → `src/app/utils/*`
  - `@environments/*` → `src/environments/*`

### Code Quality Tools
- **ESLint**: Linting with TypeScript support
- **Prettier**: Code formatting
- **Husky**: Git hooks for pre-commit validation
- **lint-staged**: Run linters on staged files

### Angular Features
- **Standalone Components**: All components use standalone API
- **Signals API**: Reactive state management with Angular Signals
- **Lazy Loading**: Route-based code splitting
- **OnPush Change Detection**: Performance optimization

## Installation

```bash
npm install
```

## Development

```bash
npm start
```

The development server will start at `http://localhost:4200`

## Building

### Development Build
```bash
npm run build
```

### Production Build
```bash
npm run build -- --configuration production
```

## Code Quality

### Linting
```bash
npm run lint          # Check for linting errors
npm run lint:fix      # Fix linting errors automatically
```

### Formatting
```bash
npm run format        # Format all files
npm run format:check  # Check formatting without changes
```

## Testing

```bash
npm test
```

## Pre-commit Hooks

The project uses Husky for git hooks. Before committing:
1. ESLint will check TypeScript files
2. Prettier will format code
3. lint-staged will run on staged files only

To set up hooks after cloning:
```bash
npx husky install
```

## Project Structure

```
src/
├── app/
│   ├── components/      # Reusable UI components
│   ├── services/        # Business logic services
│   ├── models/          # TypeScript interfaces and types
│   ├── store/           # NgRx state management
│   ├── utils/           # Utility functions
│   ├── app.ts           # Root component (standalone)
│   ├── app.config.ts    # Application configuration
│   └── app.routes.ts    # Route definitions
├── environments/        # Environment configurations
├── main.ts             # Application bootstrap
├── styles.css          # Global styles
└── index.html          # HTML entry point
```

## Key Technologies

- **Angular 21**: Frontend framework
- **Vite**: Build tool
- **TypeScript 5.9**: Type-safe JavaScript
- **Tailwind CSS**: Utility-first CSS framework
- **RxJS**: Reactive programming
- **NgRx**: State management (to be configured)
- **Vitest**: Unit testing framework

## Performance Targets

- Initial load time: < 2 seconds
- Lighthouse score: > 94
- Bundle size: < 2.5MB
- Real-time sync: < 500ms

## Next Steps

1. Install dependencies: `npm install`
2. Set up Husky hooks: `npx husky install`
3. Start development: `npm start`
4. Begin implementing features according to the task plan
