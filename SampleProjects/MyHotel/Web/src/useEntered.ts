import { useState } from 'react';

export function useEntered<Fields extends object>(initial: Fields) {
  const [entered, setEntered] = useState(initial);

  const enter = (fields: Partial<Fields>) => setEntered(current => ({ ...current, ...fields }));

  const clear = () => setEntered(initial);

  return { entered, enter, clear };
}

export const hasEmptyField = (fields: object) =>
  Object.values(fields).some(value => value === '' || value === undefined);
