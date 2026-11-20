import { QueryClientProvider } from '@tanstack/react-query';
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { App } from './App';
import i18n from './i18n';
import { newQueryClient } from './queries';
import './styles.css';

document.title = i18n.t('app.title');

const root = document.getElementById('root');
if (!root)
  throw new Error('index.html holds no element with the id root.');

createRoot(root).render(
  <StrictMode>
    <QueryClientProvider client={newQueryClient()}>
      <App />
    </QueryClientProvider>
  </StrictMode>,
);
