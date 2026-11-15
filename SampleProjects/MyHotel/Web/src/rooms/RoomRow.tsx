import { useState } from 'react';
import type { Room } from './roomsApi';

type Props = {
  room: Room;
  onSave: (room: Room) => void;
  onDelete: (roomNumber: string) => void;
};

export function RoomRow({ room, onSave, onDelete }: Props) {
  const [bedCount, setBedCount] = useState(room.bedCount);

  return (
    <tr>
      <td>{room.roomNumber}</td>
      <td>
        <input
          type="number"
          aria-label={`Beds in room ${room.roomNumber}`}
          value={bedCount}
          onChange={event => setBedCount(Number(event.target.value))}
        />
      </td>
      <td>
        <button onClick={() => onSave({ ...room, bedCount })}>Save</button>
        <button onClick={() => onDelete(room.roomNumber)}>Delete</button>
      </td>
    </tr>
  );
}
