import react from '@vitejs/plugin-react';
import { defineConfig } from 'vitest/config';

const servers = {
  development: { port: 5173, api: 'http://localhost:5167' },
  // A check runs beside the PO's own app, against a Host with data of its own: see .claude/launch.json.
  check: { port: 5174, api: 'http://localhost:5168' },
};

export default defineConfig(({ mode }) => {
  const { port, api } = mode === 'check' ? servers.check : servers.development;
  return {
    plugins: [react()],
    server: {
      port,
      strictPort: true,
      proxy: { '/rooms': api, '/room-categories': api, '/bookings': api, '/rules': api },
    },
    test: {
      environment: 'jsdom',
      setupFiles: './src/test/setup.ts',
    },
  };
});
