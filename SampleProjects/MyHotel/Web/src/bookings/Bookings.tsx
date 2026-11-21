import { useTranslation } from 'react-i18next';
import { ErrorMessage } from '../ErrorMessage';
import { useBookings, useReload, useRooms } from '../queries';
import { useAttempt } from '../useAttempt';
import { BookingRow } from './BookingRow';
import { BookRoomForm } from './BookRoomForm';
import {
  bookRoom,
  cancelBooking,
  changeBooking,
  checkIn,
  checkOut,
  pay,
  type BookingAction,
  type BookingRequest,
} from './bookingsApi';

export function Bookings() {
  const { t } = useTranslation();
  const { bookings, failure: loadFailure } = useBookings();
  const { rooms } = useRooms();
  const reload = useReload('booking');
  const { failure, attempt } = useAttempt();

  const book = async (request: BookingRequest) => {
    await bookRoom(request);
    await reload();
  };

  const save = async (bookingNumber: number, request: BookingRequest) => {
    await changeBooking(bookingNumber, request);
    await reload();
  };

  const take = (action: BookingAction, bookingNumber: number) => attempt(async () => {
    await calls[action](bookingNumber);
    await reload();
  });

  return (
    <section>
      <h2>{t('bookings.heading')}</h2>
      <ErrorMessage code={failure ?? loadFailure} />
      <table>
        <thead>
          <tr>
            <th>{t('bookings.booking')}</th>
            <th>{t('bookings.room')}</th>
            <th>{t('bookings.guests')}</th>
            <th>{t('bookings.guest')}</th>
            <th>{t('bookings.from')}</th>
            <th>{t('bookings.to')}</th>
            <th>{t('bookings.price')}</th>
            <th />
          </tr>
        </thead>
        <tbody>
          {bookings.map(booking => (
            <BookingRow key={booking.bookingNumber} booking={booking} rooms={rooms} onAction={take} onSave={save} />
          ))}
        </tbody>
      </table>
      <BookRoomForm rooms={rooms} onBook={book} />
    </section>
  );
}

const calls: Record<BookingAction, (bookingNumber: number) => Promise<void>> = {
  Cancel: cancelBooking,
  CheckIn: checkIn,
  CheckOut: checkOut,
  Pay: pay,
};
