import { Bookings } from './bookings/Bookings';
import { Rooms } from './rooms/Rooms';

export function App() {
  return (
    <main>
      <h1>My Hotel</h1>
      <Rooms />
      <Bookings />
    </main>
  );
}
