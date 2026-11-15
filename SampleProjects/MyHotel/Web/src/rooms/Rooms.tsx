import { useEffect, useState } from 'react';
import { useAttempt } from '../useAttempt';
import { AddRoomForm } from './AddRoomForm';
import { RoomRow } from './RoomRow';
import { addRoom, deleteRoom, listRooms, updateRoom, type Room } from './roomsApi';

export function Rooms() {
  const [rooms, setRooms] = useState<Room[]>([]);
  const { error, attempt } = useAttempt();

  useEffect(() => {
    attempt(async () => setRooms(await listRooms()));
  }, []);

  const add = (room: Room) => attempt(async () => {
    const added = await addRoom(room);
    setRooms(current => [...current, added]);
  });

  const save = (room: Room) => attempt(async () => {
    const saved = await updateRoom(room);
    setRooms(current => current.map(each => (each.roomNumber === saved.roomNumber ? saved : each)));
  });

  const remove = (roomNumber: string) => attempt(async () => {
    await deleteRoom(roomNumber);
    setRooms(current => current.filter(each => each.roomNumber !== roomNumber));
  });

  return (
    <section>
      <h2>Rooms</h2>
      {error && <p role="alert">{error}</p>}
      <table>
        <thead>
          <tr>
            <th>Room</th>
            <th>Beds</th>
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
