import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { ChangeRoomCategoryRow } from './ChangeRoomCategoryRow';
import type { RoomCategory, RoomCategoryOverview } from './roomCategoriesApi';

type Props = {
  roomCategory: RoomCategoryOverview;
  asking: boolean;
  onSave: (roomCategory: RoomCategory) => Promise<void>;
  onDelete: (name: string) => void;
};

export function RoomCategoryRow({ roomCategory, asking, onSave, onDelete }: Props) {
  const { t } = useTranslation();
  const [changing, setChanging] = useState(false);

  if (changing)
    return <ChangeRoomCategoryRow roomCategory={roomCategory} onSave={onSave} onClose={() => setChanging(false)} />;

  return (
    <tr className={asking ? 'asking' : undefined}>
      <td>{roomCategory.name}</td>
      <td>{roomCategory.description}</td>
      <td>
        <button
          disabled={roomCategory.hasRooms}
          title={roomCategory.hasRooms ? t('errors.RoomCategoryHasRooms') : undefined}
          onClick={() => onDelete(roomCategory.name)}
        >
          {t('roomCategories.delete')}
        </button>
        <button onClick={() => setChanging(true)}>{t('general.change')}</button>
      </td>
    </tr>
  );
}
