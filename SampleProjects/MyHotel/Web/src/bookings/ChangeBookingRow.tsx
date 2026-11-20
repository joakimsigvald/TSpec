import { useId } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorMessage } from '../ErrorMessage';
import type { Room } from '../rooms/roomsApi';
import { hasEmptyField, useEntered } from '../useEntered';
import { useFormAttempt } from '../useFormAttempt';
import { fieldOf, inAlphanumericOrder, requestOf, roomNumbersOf, sameEntry, type BookingEntry } from './bookingEntry';
import type { Booking, BookingRequest } from './bookingsApi';

type Props = {
  booking: Booking;
  rooms: Room[];
  onSave: (request: BookingRequest) => Promise<void>;
  onClose: () => void;
};

// A table row cannot hold a form, so each field is named for the booking it changes.
export function ChangeBookingRow({ booking, rooms, onSave, onClose }: Props) {
  const { t } = useTranslation();
  const { attempt, failureAt } = useFormAttempt(fieldOf);
  const original = entryOf(booking);
  const { entered, enter } = useEntered(original);
  const roomNumberError = useId();
  const guestCountError = useId();
  const toError = useId();
  const buttonError = useId();
  const nameOf = (key: string) => t(key, { bookingNumber: booking.bookingNumber });

  const save = async () => {
    if (await attempt(() => onSave(requestOf(entered))))
      onClose();
  };

  return (
    <tr className="changing">
      <td>{booking.bookingNumber}</td>
      <td>
        <select
          aria-label={nameOf('bookings.roomInBooking')}
          value={entered.roomNumber}
          onChange={event => enter({ roomNumber: event.target.value })}
          aria-describedby={roomNumberError}
        >
          {inAlphanumericOrder(roomNumbersWith(rooms, booking.info.roomNumber)).map(roomNumber => (
            <option key={roomNumber}>{roomNumber}</option>
          ))}
        </select>
        <ErrorMessage id={roomNumberError} code={failureAt('roomNumber')} />
      </td>
      <td>
        <input
          type="number"
          required
          aria-label={nameOf('bookings.guestsInBooking')}
          value={entered.guestCount}
          onChange={event => enter({ guestCount: event.target.value })}
          aria-describedby={guestCountError}
        />
        <ErrorMessage id={guestCountError} code={failureAt('guestCount')} />
      </td>
      <td>
        <input
          className="narrow"
          required
          aria-label={nameOf('bookings.guestInBooking')}
          value={entered.guestName}
          onChange={event => enter({ guestName: event.target.value })}
        />
      </td>
      <td>
        <input
          type="date"
          required
          aria-label={nameOf('bookings.fromInBooking')}
          value={entered.from}
          onChange={event => enter({ from: event.target.value })}
        />
      </td>
      <td>
        <input
          type="date"
          required
          aria-label={nameOf('bookings.toInBooking')}
          value={entered.to}
          onChange={event => enter({ to: event.target.value })}
          aria-describedby={toError}
        />
        <ErrorMessage id={toError} code={failureAt('to')} />
      </td>
      <td>
        <button
          disabled={sameEntry(entered, original) || hasEmptyField(entered)}
          onClick={save}
          aria-describedby={buttonError}
        >
          {t('general.save')}
        </button>
        <button onClick={onClose}>{t('general.cancelChange')}</button>
        <ErrorMessage id={buttonError} code={failureAt('button')} />
      </td>
    </tr>
  );
}

// The booking's room may have been deleted since, and the field still holds it.
const roomNumbersWith = (rooms: Room[], roomNumber: string) => [...new Set([...roomNumbersOf(rooms), roomNumber])];

const entryOf = ({ info, period }: Booking): BookingEntry => ({
  from: period.from,
  to: period.to,
  roomNumber: info.roomNumber,
  guestCount: String(info.guestCount),
  guestName: info.guestName,
});
