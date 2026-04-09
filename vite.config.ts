import { defineConfig } from 'vite';
import angular from '@angular/build';

export default defineConfig({
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
});
