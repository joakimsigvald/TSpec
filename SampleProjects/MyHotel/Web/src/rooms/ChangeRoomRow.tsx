import { useId, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorMessage } from '../ErrorMessage';
import type { RoomCategory } from '../roomCategories/roomCategoriesApi';
import { useFormAttempt } from '../useFormAttempt';
import { roomFieldOf } from './roomFieldOf';
import type { Room } from './roomsApi';

type Props = {
  room: Room;
  roomCategories: RoomCategory[];
  bedCounts: number[];
  onSave: (room: Room) => Promise<void>;
  onClose: () => void;
};

export function ChangeRoomRow({ room, roomCategories, bedCounts, onSave, onClose }: Props) {
  const { t } = useTranslation();
  const { attempt, failureAt } = useFormAttempt(roomFieldOf);
  const [bedCount, setBedCount] = useState(room.bedCount);
  const [roomCategory, setRoomCategory] = useState(room.roomCategory);
  const [pricePerNight, setPricePerNight] = useState(String(room.pricePerNight));
  const unchanged = bedCount === room.bedCount
    && roomCategory === room.roomCategory
    && pricePerNight === String(room.pricePerNight);
  const roomCategoryError = useId();
  const bedCountError = useId();
  const pricePerNightError = useId();
  const buttonError = useId();

  const save = async () => {
    const changed = { roomNumber: room.roomNumber, bedCount, roomCategory, pricePerNight: Number(pricePerNight) };
    if (await attempt(() => onSave(changed)))
      onClose();
  };

  return (
    <tr className="changing">
      <td>{room.roomNumber}</td>
      <td>
        <select
          aria-label={t('rooms.roomCategoryOf', { roomNumber: room.roomNumber })}
          value={roomCategory}
          onChange={event => setRoomCategory(event.target.value)}
          aria-describedby={roomCategoryError}
        >
          {roomCategories.map(category => <option key={category.name}>{category.name}</option>)}
        </select>
        <ErrorMessage id={roomCategoryError} code={failureAt('roomCategory')} />
      </td>
      <td>
        <select
          aria-label={t('rooms.bedsInRoom', { roomNumber: room.roomNumber })}
          value={bedCount}
          onChange={event => setBedCount(Number(event.target.value))}
          aria-describedby={bedCountError}
        >
          {bedCounts.map(count => <option key={count}>{count}</option>)}
        </select>
        <ErrorMessage id={bedCountError} code={failureAt('bedCount')} />
      </td>
      <td>
        <input
          type="number"
          required
          aria-label={t('rooms.pricePerNightOf', { roomNumber: room.roomNumber })}
          value={pricePerNight}
          onChange={event => setPricePerNight(event.target.value)}
          aria-describedby={pricePerNightError}
        />
        <ErrorMessage id={pricePerNightError} code={failureAt('pricePerNight')} />
      </td>
      <td>
        <button disabled={unchanged || pricePerNight === ''} onClick={save} aria-describedby={buttonError}>
          {t('general.save')}
        </button>
        <button onClick={onClose}>{t('general.cancelChange')}</button>
        <ErrorMessage id={buttonError} code={failureAt('button')} />
      </td>
    </tr>
  );
}
