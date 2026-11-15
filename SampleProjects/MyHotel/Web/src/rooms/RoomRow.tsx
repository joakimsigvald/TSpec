import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import type { Room } from './roomsApi';

type Props = {
  room: Room;
  onSave: (room: Room) => void;
  onDelete: (roomNumber: string) => void;
};

export function RoomRow({ room, onSave, onDelete }: Props) {
  const { t } = useTranslation();
  const [bedCount, setBedCount] = useState(room.bedCount);

  return (
    <tr>
      <td>{room.roomNumber}</td>
      <td>
        <input
          type="number"
          aria-label={t('rooms.bedsInRoom', { roomNumber: room.roomNumber })}
          value={bedCount}
          onChange={event => setBedCount(Number(event.target.value))}
        />
      </td>
      <td>
        <button onClick={() => onSave({ ...room, bedCount })}>{t('rooms.save')}</button>
        <button onClick={() => onDelete(room.roomNumber)}>{t('rooms.delete')}</button>
      </td>
    </tr>
  );
}
