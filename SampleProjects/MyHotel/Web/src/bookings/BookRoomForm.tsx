import type { FormEvent } from 'react';
import type { BookingRequest } from './bookingsApi';

type Props = { onBook: (request: BookingRequest) => Promise<boolean> };

export function BookRoomForm({ onBook }: Props) {
  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = event.currentTarget;
    if (await onBook(requestIn(new FormData(form))))
      form.reset();
  };

  return (
    <form onSubmit={submit}>
      <label>
        Room number
        <input name="roomNumber" required />
      </label>
      <label>
        Guest
        <input name="guestName" required />
      </label>
      <label>
        From
        <input name="from" type="date" required />
      </label>
      <label>
        To
        <input name="to" type="date" required />
      </label>
      <button>Book</button>
    </form>
  );
}

const requestIn = (data: FormData): BookingRequest => ({
  roomNumber: String(data.get('roomNumber')),
  guestName: String(data.get('guestName')),
  from: String(data.get('from')),
  to: String(data.get('to')),
});
