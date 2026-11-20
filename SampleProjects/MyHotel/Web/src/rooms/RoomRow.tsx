import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import type { RoomCategory } from '../roomCategories/roomCategoriesApi';
import type { Room, RoomOverview } from './roomsApi';

type Props = {
  room: RoomOverview;
  roomCategories: RoomCategory[];
  bedCounts: number[];
  asking: boolean;
  onSave: (room: Room) => void;
  onCancelBookings: (roomNumber: string) => void;
  onDelete: (roomNumber: string) => void;
};

export function RoomRow({ room, roomCategories, bedCounts, asking, onSave, onCancelBookings, onDelete }: Props) {
  const { t } = useTranslation();
  const [bedCount, setBedCount] = useState(room.bedCount);
  const [roomCategory, setRoomCategory] = useState(room.roomCategory);
  const unchanged = bedCount === room.bedCount && roomCategory === room.roomCategory;

  return (
    <tr className={asking ? 'asking' : undefined}>
      <td>{room.roomNumber}</td>
      <td>
        <select
          aria-label={t('rooms.roomCategoryOf', { roomNumber: room.roomNumber })}
          value={roomCategory}
          onChange={event => setRoomCategory(event.target.value)}
        >
          {roomCategories.map(category => <option key={category.name}>{category.name}</option>)}
        </select>
      </td>
      <td>
        <select
          aria-label={t('rooms.bedsInRoom', { roomNumber: room.roomNumber })}
          value={bedCount}
          onChange={event => setBedCount(Number(event.target.value))}
        >
          {bedCounts.map(count => <option key={count}>{count}</option>)}
        </select>
      </td>
      <td>
        <button disabled={unchanged} onClick={() => onSave({ roomNumber: room.roomNumber, bedCount, roomCategory })}>
          {t('rooms.save')}
        </button>
        <button
          disabled={!room.hasOpenBookings}
          title={room.hasOpenBookings ? undefined : t('rooms.noOpenBookings')}
          onClick={() => onCancelBookings(room.roomNumber)}
        >
          {t('rooms.cancelBookings')}
        </button>
        <button
          disabled={room.hasOpenBookings}
          title={room.hasOpenBookings ? t('errors.RoomHasOpenBookings') : undefined}
          onClick={() => onDelete(room.roomNumber)}
        >
          {t('rooms.delete')}
        </button>
      </td>
    </tr>
  );
}
