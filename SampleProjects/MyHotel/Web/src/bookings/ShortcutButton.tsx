import type { ReactNode } from 'react';

type Props = { pressed: boolean; onClick: () => void; children: ReactNode };

export function ShortcutButton({ pressed, onClick, children }: Props) {
  return (
    <button type="button" aria-pressed={pressed} disabled={pressed} onClick={onClick}>
      {children}
    </button>
  );
}
