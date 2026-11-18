import { useId, useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorMessage } from '../ErrorMessage';
import type { Room } from '../rooms/roomsApi';
import { useFormAttempt } from '../useFormAttempt';
import { BookingPeriod } from './BookingPeriod';
import type { BookingRequest } from './bookingsApi';

type Props = { rooms: Room[]; onBook: (request: BookingRequest) => Promise<void> };

const fieldOf = {
  RoomNotFound: 'roomNumber',
  RoomAlreadyBooked: 'roomNumber',
  InvalidBookingPeriod: 'to',
} as const;

export function BookRoomForm({ rooms, onBook }: Props) {
  const { t } = useTranslation();
  const { attempt, failureAt } = useFormAttempt(fieldOf);
  const [bookingsMade, setBookingsMade] = useState(0);
  const roomNumberError = useId();
  const buttonError = useId();

  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = event.currentTarget;
    if (await attempt(() => onBook(requestIn(new FormData(form))))) {
      form.reset();
      setBookingsMade(count => count + 1);
    }
  };

  return (
    <form onSubmit={submit}>
      <div className="field">
        <label>
          {t('bookings.roomNumber')}
          <select name="roomNumber" required aria-describedby={roomNumberError}>
            <option value="">{t('bookings.chooseRoom')}</option>
            {inAlphanumericOrder(rooms).map(room => (
              <option key={room.roomNumber}>{room.roomNumber}</option>
            ))}
          </select>
        </label>
        <ErrorMessage id={roomNumberError} code={failureAt('roomNumber')} />
      </div>
      <div className="field">
        <label>
          {t('bookings.guest')}
          <input name="guestName" required />
        </label>
      </div>
      <BookingPeriod key={bookingsMade} toFailure={failureAt('to')} />
      <div className="field actions">
        <button aria-describedby={buttonError}>{t('bookings.book')}</button>
        <ErrorMessage id={buttonError} code={failureAt('button')} />
      </div>
    </form>
  );
}

const inAlphanumericOrder = (rooms: Room[]) =>
  [...rooms].sort((one, other) => one.roomNumber.localeCompare(other.roomNumber, 'sv', { numeric: true }));

const requestIn = (data: FormData): BookingRequest => ({
  roomNumber: String(data.get('roomNumber')),
  guestName: String(data.get('guestName')),
  from: String(data.get('from')),
  to: String(data.get('to')),
});
