import { useAttempt } from './useAttempt';

export function useFormAttempt<Field extends string>(fieldOf: Partial<Record<string, Field>>) {
  const { failure, attempt } = useAttempt();

  const failureAt = (place: Field | 'button') =>
    failure && (fieldOf[failure] ?? 'button') === place ? failure : undefined;

  return { attempt, failureAt };
}
