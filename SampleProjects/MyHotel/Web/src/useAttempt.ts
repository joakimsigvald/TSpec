import { useState } from 'react';
import { ApiError } from './http';

export function useAttempt() {
  const [failure, setFailure] = useState<string>();

  const attempt = async (action: () => Promise<void>): Promise<boolean> => {
    setFailure(undefined);
    try {
      await action();
      return true;
    } catch (error) {
      setFailure(codeOf(error));
      return false;
    }
  };

  return { failure, attempt };
}

export const codeOf = (error: unknown) => (error instanceof ApiError ? error.code : 'Unknown');
