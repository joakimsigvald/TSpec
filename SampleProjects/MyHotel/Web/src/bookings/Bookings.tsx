import { useTranslation } from 'react-i18next';
import { ErrorMessage } from '../ErrorMessage';
import { useBookings, useReload, useRooms } from '../queries';
import { useAttempt } from '../useAttempt';
import { BookRoomForm } from './BookRoomForm';
import { bookRoom, cancelBooking, type BookingRequest } from './bookingsApi';

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

  const cancel = (bookingNumber: number) => attempt(async () => {
    await cancelBooking(bookingNumber);
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
            <th />
          </tr>
        </thead>
        <tbody>
          {bookings.map(booking => (
            <tr key={booking.bookingNumber}>
              <td>{booking.bookingNumber}</td>
              <td>{booking.roomNumber}</td>
              <td>{booking.guestCount}</td>
              <td>{booking.guestName}</td>
              <td>{booking.period.from}</td>
              <td>{booking.period.to}</td>
              <td>
                {booking.status === 'Open' && (
                  <button onClick={() => cancel(booking.bookingNumber)}>{t('bookings.cancel')}</button>
                )}
                {booking.status === 'Cancelled' && t('bookings.cancelled')}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      <BookRoomForm rooms={rooms} onBook={book} />
    </section>
  );
}
