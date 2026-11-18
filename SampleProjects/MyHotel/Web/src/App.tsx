import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Bookings } from './bookings/Bookings';
import { Rooms } from './rooms/Rooms';
import type { Room } from './rooms/roomsApi';

export function App() {
  const { t } = useTranslation();
  const [rooms, setRooms] = useState<Room[]>([]);

  return (
    <main>
      <h1>{t('app.title')}</h1>
      <Rooms rooms={rooms} onChange={setRooms} />
      <Bookings rooms={rooms} />
    </main>
  );
}
