import { useId, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorMessage } from '../ErrorMessage';
import type { Room } from '../rooms/roomsApi';
import { useFormAttempt } from '../useFormAttempt';
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
      <div className="field">
        <label>
          {t('bookings.from')}
          <input name="from" type="date" required />
        </label>
      </div>
      <div className="field">
        <label>
          {t('bookings.to')}
          <input name="to" type="date" required aria-describedby={toError} />
        </label>
        <ErrorMessage id={toError} code={failureAt('to')} />
      </div>
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
