import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { cancelOpenBookings } from '../bookings/bookingsApi';
import { ConfirmDialog, type Confirmation } from '../ConfirmDialog';
import { ErrorMessage } from '../ErrorMessage';
import { useReload, useRooms } from '../queries';
import { useAttempt } from '../useAttempt';
import { AddRoomForm } from './AddRoomForm';
import { RoomRow } from './RoomRow';
import { addRoom, deleteRoom, updateRoom, type Room } from './roomsApi';

export function Rooms() {
  const { t } = useTranslation();
  const { rooms, failure: loadFailure } = useRooms();
  const reload = useReload('room');
  const reloadAfterBookings = useReload('booking');
  const { failure, attempt } = useAttempt();
  const [asking, setAsking] = useState<Asking>();

  const add = async (room: Room) => {
    await addRoom(room);
    await reload();
  };

  const save = (room: Room) => attempt(async () => {
    await updateRoom(room);
    await reload();
  });

  const cancelBookings = (roomNumber: string) => setAsking({
    roomNumber,
    confirmation: {
      caption: t('rooms.confirmCancelBookingsCaption'),
      question: t('rooms.confirmCancelBookings', { roomNumber }),
      onConfirm: () => attempt(async () => {
        await cancelOpenBookings(roomNumber);
        await reloadAfterBookings();
      }),
    },
  });

  const remove = (roomNumber: string) => setAsking({
    roomNumber,
    confirmation: {
      caption: t('rooms.confirmDeleteCaption'),
      question: t('rooms.confirmDelete', { roomNumber }),
      onConfirm: () => attempt(async () => {
        await deleteRoom(roomNumber);
        await reload();
      }),
    },
  });

  return (
    <section>
      <h2>{t('rooms.heading')}</h2>
      <ErrorMessage code={failure ?? loadFailure} />
      <table>
        <thead>
          <tr>
            <th>{t('rooms.room')}</th>
            <th>{t('rooms.beds')}</th>
            <th />
          </tr>
        </thead>
        <tbody>
          {rooms.map(room => (
            <RoomRow
              key={room.roomNumber}
              room={room}
              asking={asking?.roomNumber === room.roomNumber}
              onSave={save}
              onCancelBookings={cancelBookings}
              onDelete={remove}
            />
          ))}
        </tbody>
      </table>
      <AddRoomForm onAdd={add} />
      {asking && <ConfirmDialog {...asking.confirmation} onClose={() => setAsking(undefined)} />}
    </section>
  );
}

type Asking = { roomNumber: string; confirmation: Confirmation };
