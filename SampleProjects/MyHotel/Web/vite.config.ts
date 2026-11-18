import react from '@vitejs/plugin-react';
import { defineConfig } from 'vitest/config';

const api = 'http://localhost:5167';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    strictPort: true,
    proxy: { '/rooms': api, '/room-categories': api, '/bookings': api },
  },
  test: {
    environment: 'jsdom',
    setupFiles: './src/test/setup.ts',
  },
});
