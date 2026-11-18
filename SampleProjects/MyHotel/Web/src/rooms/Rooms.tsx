import { useEffect, type Dispatch, type SetStateAction } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorMessage } from '../ErrorMessage';
import { useAttempt } from '../useAttempt';
import { AddRoomForm } from './AddRoomForm';
import { RoomRow } from './RoomRow';
import { addRoom, deleteRoom, listRooms, updateRoom, type Room } from './roomsApi';

type Props = { rooms: Room[]; onChange: Dispatch<SetStateAction<Room[]>> };

export function Rooms({ rooms, onChange }: Props) {
  const { t } = useTranslation();
  const { failure, attempt } = useAttempt();

  useEffect(() => {
    attempt(async () => onChange(await listRooms()));
  }, []);

  const add = async (room: Room) => {
    const added = await addRoom(room);
    onChange(current => [...current, added]);
  };

  const save = (room: Room) => attempt(async () => {
    const saved = await updateRoom(room);
    onChange(current => current.map(each => (each.roomNumber === saved.roomNumber ? saved : each)));
  });

  const remove = (roomNumber: string) => attempt(async () => {
    await deleteRoom(roomNumber);
    onChange(current => current.filter(each => each.roomNumber !== roomNumber));
  });

  return (
    <section>
      <h2>{t('rooms.heading')}</h2>
      <ErrorMessage code={failure} />
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
            <RoomRow key={room.roomNumber} room={room} onSave={save} onDelete={remove} />
          ))}
        </tbody>
      </table>
      <AddRoomForm onAdd={add} />
    </section>
  );
}
