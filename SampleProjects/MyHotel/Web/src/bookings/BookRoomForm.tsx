import { useId, useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorMessage } from '../ErrorMessage';
import type { Room } from '../rooms/roomsApi';
import { hasEmptyField, useEntered } from '../useEntered';
import { useFormAttempt } from '../useFormAttempt';
import { fieldOf, inAlphanumericOrder, requestOf, roomNumbersOf, type BookingEntry } from './bookingEntry';
import { BookingPeriodFields } from './BookingPeriodFields';
import type { BookingRequest } from './bookingsApi';

type Props = { rooms: Room[]; onBook: (request: BookingRequest) => Promise<void> };

const nothingEntered: BookingEntry = { from: '', to: '', roomNumber: '', guestCount: '', guestName: '' };

export function BookRoomForm({ rooms, onBook }: Props) {
  const { t } = useTranslation();
  const { attempt, failureAt } = useFormAttempt(fieldOf);
  const { entered, enter, clear } = useEntered(nothingEntered);
  const [bookingsMade, setBookingsMade] = useState(0);
  const roomNumberError = useId();
  const guestCountError = useId();
  const buttonError = useId();

  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (await attempt(() => onBook(requestOf(entered)))) {
      clear();
      setBookingsMade(count => count + 1);
    }
  };

  return (
    <form onSubmit={submit}>
      <BookingPeriodFields
        key={bookingsMade}
        period={{ from: entered.from, to: entered.to }}
        onChange={enter}
        toFailure={failureAt('to')}
      />
      <div className="field">
        <label>
          {t('bookings.roomNumber')}
          <select
            required
            value={entered.roomNumber}
            onChange={event => enter({ roomNumber: event.target.value })}
            aria-describedby={roomNumberError}
          >
            <option value="">{t('bookings.chooseRoom')}</option>
            {inAlphanumericOrder(roomNumbersOf(rooms)).map(roomNumber => (
              <option key={roomNumber}>{roomNumber}</option>
            ))}
          </select>
        </label>
        <ErrorMessage id={roomNumberError} code={failureAt('roomNumber')} />
      </div>
      <div className="field">
        <label>
          {t('bookings.guestCount')}
          <input
            type="number"
            required
            value={entered.guestCount}
            onChange={event => enter({ guestCount: event.target.value })}
            aria-describedby={guestCountError}
          />
        </label>
        <ErrorMessage id={guestCountError} code={failureAt('guestCount')} />
      </div>
      <div className="field">
        <label>
          {t('bookings.guest')}
          <input required value={entered.guestName} onChange={event => enter({ guestName: event.target.value })} />
        </label>
      </div>
      <div className="field actions">
        <button disabled={hasEmptyField(entered)} aria-describedby={buttonError}>{t('bookings.book')}</button>
        <ErrorMessage id={buttonError} code={failureAt('button')} />
      </div>
    </form>
  );
}
