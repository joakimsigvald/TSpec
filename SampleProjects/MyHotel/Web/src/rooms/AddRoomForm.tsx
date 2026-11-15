import { useId, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorMessage } from '../ErrorMessage';
import { useFormAttempt } from '../useFormAttempt';
import type { Room } from './roomsApi';

type Props = { onAdd: (room: Room) => Promise<void> };

const fieldOf = { RoomAlreadyExists: 'roomNumber' } as const;

export function AddRoomForm({ onAdd }: Props) {
  const { t } = useTranslation();
  const { attempt, failureAt } = useFormAttempt(fieldOf);
  const roomNumberError = useId();
  const buttonError = useId();

  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = event.currentTarget;
    if (await attempt(() => onAdd(roomIn(new FormData(form)))))
      form.reset();
  };

  return (
    <form onSubmit={submit}>
      <div className="field">
        <label>
          {t('rooms.roomNumber')}
          <input name="roomNumber" required aria-describedby={roomNumberError} />
        </label>
        <ErrorMessage id={roomNumberError} code={failureAt('roomNumber')} />
      </div>
      <div className="field">
        <label>
          {t('rooms.beds')}
          <input name="bedCount" type="number" required />
        </label>
      </div>
      <div className="field actions">
        <button aria-describedby={buttonError}>{t('rooms.add')}</button>
        <ErrorMessage id={buttonError} code={failureAt('button')} />
      </div>
    </form>
  );
}

const roomIn = (data: FormData): Room => ({
  roomNumber: String(data.get('roomNumber')),
  bedCount: Number(data.get('bedCount')),
});
