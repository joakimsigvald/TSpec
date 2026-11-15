import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { App } from './App';
import i18n from './i18n';
import './styles.css';

document.title = i18n.t('app.title');

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>,
);
