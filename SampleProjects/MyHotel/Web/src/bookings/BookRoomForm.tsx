import { useId, type FormEvent } from 'react';
import { ErrorMessage } from '../ErrorMessage';
import { useFormAttempt } from '../useFormAttempt';
import type { BookingRequest } from './bookingsApi';

type Props = { onBook: (request: BookingRequest) => Promise<void> };

const fieldOf = {
  RoomNotFound: 'roomNumber',
  RoomAlreadyBooked: 'roomNumber',
  InvalidBookingPeriod: 'to',
} as const;

export function BookRoomForm({ onBook }: Props) {
  const { attempt, failureAt } = useFormAttempt(fieldOf);
  const roomNumberError = useId();
  const toError = useId();
  const buttonError = useId();

  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = event.currentTarget;
    if (await attempt(() => onBook(requestIn(new FormData(form)))))
      form.reset();
  };

  return (
    <form onSubmit={submit}>
      <div className="field">
        <label>
          Room number
          <input name="roomNumber" required aria-describedby={roomNumberError} />
        </label>
        <ErrorMessage id={roomNumberError} code={failureAt('roomNumber')} />
      </div>
      <div className="field">
        <label>
          Guest
          <input name="guestName" required />
        </label>
      </div>
      <div className="field">
        <label>
          From
          <input name="from" type="date" required />
        </label>
      </div>
      <div className="field">
        <label>
          To
          <input name="to" type="date" required aria-describedby={toError} />
        </label>
        <ErrorMessage id={toError} code={failureAt('to')} />
      </div>
      <div className="field actions">
        <button aria-describedby={buttonError}>Book</button>
        <ErrorMessage id={buttonError} code={failureAt('button')} />
      </div>
    </form>
  );
}

const requestIn = (data: FormData): BookingRequest => ({
  roomNumber: String(data.get('roomNumber')),
  guestName: String(data.get('guestName')),
  from: String(data.get('from')),
  to: String(data.get('to')),
});
