import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import type { RoomCategory } from '../roomCategories/roomCategoriesApi';
import { ChangeRoomRow } from './ChangeRoomRow';
import type { Room, RoomOverview } from './roomsApi';

type Props = {
  room: RoomOverview;
  roomCategories: RoomCategory[];
  bedCounts: number[];
  asking: boolean;
  onSave: (room: Room) => Promise<void>;
  onCancelBookings: (roomNumber: string) => void;
  onDelete: (roomNumber: string) => void;
};

export function RoomRow({ room, roomCategories, bedCounts, asking, onSave, onCancelBookings, onDelete }: Props) {
  const { t } = useTranslation();
  const [changing, setChanging] = useState(false);

  if (changing)
    return (
      <ChangeRoomRow
        room={room}
        roomCategories={roomCategories}
        bedCounts={bedCounts}
        onSave={onSave}
        onClose={() => setChanging(false)}
      />
    );

  return (
    <tr className={asking ? 'asking' : undefined}>
      <td>{room.roomNumber}</td>
      <td>{room.roomCategory}</td>
      <td>{room.bedCount}</td>
      <td>
        <button
          disabled={!room.hasCancellableBookings}
          title={room.hasCancellableBookings ? undefined : t('rooms.noCancellableBookings')}
          onClick={() => onCancelBookings(room.roomNumber)}
        >
          {t('rooms.cancelBookings')}
        </button>
        <button
          disabled={room.held}
          title={room.held ? t('errors.RoomHeld') : undefined}
          onClick={() => onDelete(room.roomNumber)}
        >
          {t('rooms.delete')}
        </button>
        <button onClick={() => setChanging(true)}>{t('general.change')}</button>
      </td>
    </tr>
  );
}
