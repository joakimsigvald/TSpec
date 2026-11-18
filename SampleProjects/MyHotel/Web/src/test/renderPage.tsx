import { QueryClientProvider } from '@tanstack/react-query';
import { render } from '@testing-library/react';
import type { ReactNode } from 'react';
import { newQueryClient } from '../queries';

export const renderPage = (ui: ReactNode) =>
  render(<QueryClientProvider client={newQueryClient()}>{ui}</QueryClientProvider>);
