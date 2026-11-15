import { useEffect, useState } from 'react';
import { ErrorMessage } from '../ErrorMessage';
import { useAttempt } from '../useAttempt';
import { BookRoomForm } from './BookRoomForm';
import { bookRoom, cancelBooking, listBookings, type Booking, type BookingRequest } from './bookingsApi';

export function Bookings() {
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
      <h2>Bookings</h2>
      <ErrorMessage code={failure} />
      <table>
        <thead>
          <tr>
            <th>Booking</th>
            <th>Room</th>
            <th>Guest</th>
            <th>From</th>
            <th>To</th>
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
                <button onClick={() => cancel(booking.bookingNumber)}>Cancel</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      <BookRoomForm onBook={book} />
    </section>
  );
}
