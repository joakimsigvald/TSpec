import { useState } from 'react';

export function useEntered<Fields extends object>(nothingEntered: Fields) {
  const [entered, setEntered] = useState(nothingEntered);

  const enter = (fields: Partial<Fields>) => setEntered(current => ({ ...current, ...fields }));

  const clear = () => setEntered(nothingEntered);

  return { entered, enter, clear };
}

export const hasEmptyField = (fields: object) =>
  Object.values(fields).some(value => value === '' || value === undefined);
