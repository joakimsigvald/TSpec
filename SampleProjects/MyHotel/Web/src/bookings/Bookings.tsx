import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorMessage } from '../ErrorMessage';
import { useAttempt } from '../useAttempt';
import { BookRoomForm } from './BookRoomForm';
import { bookRoom, cancelBooking, listBookings, type Booking, type BookingRequest } from './bookingsApi';

export function Bookings() {
  const { t } = useTranslation();
  const [bookings, setBookings] = useState<Booking[]>([]);
  const { failure, attempt } = useAttempt();

  useEffect(() => {
    attempt(async () => setBookings(await listBookings()));
  }, []);

  const book = async (request: BookingRequest) => {
    const booked = await bookRoom(request);
    setBookings(current => [...current, booked]);
  };

  const cancel = (bookingNumber: number) => attempt(async () => {
    await cancelBooking(bookingNumber);
    setBookings(current => current.filter(each => each.bookingNumber !== bookingNumber));
  });

  return (
    <section>
      <h2>{t('bookings.heading')}</h2>
      <ErrorMessage code={failure} />
      <table>
        <thead>
          <tr>
            <th>{t('bookings.booking')}</th>
            <th>{t('bookings.room')}</th>
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
              <td>{booking.guestName}</td>
              <td>{booking.from}</td>
              <td>{booking.to}</td>
              <td>
                <button onClick={() => cancel(booking.bookingNumber)}>{t('bookings.cancel')}</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      <BookRoomForm onBook={book} />
    </section>
  );
}
