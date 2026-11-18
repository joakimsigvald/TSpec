import { useTranslation } from 'react-i18next';
import { Bookings } from './bookings/Bookings';
import { RoomCategories } from './roomCategories/RoomCategories';
import { Rooms } from './rooms/Rooms';

export function App() {
  const { t } = useTranslation();

  return (
    <main>
      <h1>{t('app.title')}</h1>
      <RoomCategories />
      <Rooms />
      <Bookings />
    </main>
  );
}
