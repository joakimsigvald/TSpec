import { useState } from 'react';

export function useAttempt() {
  const [error, setError] = useState<string>();

  const attempt = async (action: () => Promise<void>): Promise<boolean> => {
    setError(undefined);
    try {
      await action();
      return true;
    } catch (failure) {
      setError((failure as Error).message);
      return false;
    }
  };

  return { error, attempt };
}
