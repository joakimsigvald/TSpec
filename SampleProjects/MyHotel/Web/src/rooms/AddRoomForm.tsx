import { useId, type FormEvent } from 'react';
import { ErrorMessage } from '../ErrorMessage';
import { useAttempt } from '../useAttempt';
import type { Room } from './roomsApi';

type Props = { onAdd: (room: Room) => Promise<void> };

export function AddRoomForm({ onAdd }: Props) {
  const { failure, attempt } = useAttempt();
  const roomNumberError = useId();
  const formError = useId();
  const aboutRoomNumber = failure === 'RoomAlreadyExists';

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
          Room number
          <input name="roomNumber" required aria-describedby={roomNumberError} />
        </label>
        <ErrorMessage id={roomNumberError} code={aboutRoomNumber ? failure : undefined} />
      </div>
      <div className="field">
        <label>
          Beds
          <input name="bedCount" type="number" required />
        </label>
      </div>
      <div className="field">
        <button aria-describedby={formError}>Add room</button>
        <ErrorMessage id={formError} code={aboutRoomNumber ? undefined : failure} />
      </div>
    </form>
  );
}

const roomIn = (data: FormData): Room => ({
  roomNumber: String(data.get('roomNumber')),
  bedCount: Number(data.get('bedCount')),
});
