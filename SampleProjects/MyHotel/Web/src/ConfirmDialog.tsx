import { useEffect, useId, useRef } from 'react';
import { useTranslation } from 'react-i18next';

export type Confirmation = { caption: string; question: string; onConfirm: () => void };

type Props = Confirmation & { onClose: () => void };

export function ConfirmDialog({ caption, question, onConfirm, onClose }: Props) {
  const { t } = useTranslation();
  const dialog = useRef<HTMLDialogElement>(null);
  const captionId = useId();
  const questionId = useId();

  // StrictMode runs the effect twice, and the second call must not reopen an open dialog.
  useEffect(() => {
    if (!dialog.current?.open)
      dialog.current?.showModal();
  }, []);

  const confirm = () => {
    onClose();
    onConfirm();
  };

  return (
    <dialog ref={dialog} aria-labelledby={captionId} aria-describedby={questionId} onClose={onClose}>
      <h3 id={captionId}>{caption}</h3>
      <p id={questionId}>{question}</p>
      <div className="answers">
        <button onClick={confirm}>{t('app.ok')}</button>
        <button onClick={onClose}>{t('app.cancel')}</button>
      </div>
    </dialog>
  );
}
