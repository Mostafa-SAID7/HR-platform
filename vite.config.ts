import { defineConfig } from 'vite';
import tsconfigPaths from 'vite-tsconfig-paths';

export default defineConfig({
  plugins: [tsconfigPaths()],
  build: {
    target: 'ES2022',
    outDir: 'dist',
    sourcemap: true,
    minify: 'esbuild',
  },
  server: {
    port: 4200,
    open: true,
  },
  preview: {
    port: 4200,
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['src/test-setup.ts'],
  },
});
