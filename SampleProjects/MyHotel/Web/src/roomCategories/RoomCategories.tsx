import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { ConfirmDialog, type Confirmation } from '../ConfirmDialog';
import { ErrorMessage } from '../ErrorMessage';
import { useReload, useRoomCategories } from '../queries';
import { useAttempt } from '../useAttempt';
import { AddRoomCategoryForm } from './AddRoomCategoryForm';
import { RoomCategoryRow } from './RoomCategoryRow';
import {
  addRoomCategory,
  deleteRoomCategory,
  updateRoomCategory,
  type RoomCategory,
} from './roomCategoriesApi';

export function RoomCategories() {
  const { t } = useTranslation();
  const { roomCategories, failure: loadFailure } = useRoomCategories();
  const reload = useReload('roomCategory');
  const { failure, attempt } = useAttempt();
  const [asking, setAsking] = useState<Asking>();

  const add = async (roomCategory: RoomCategory) => {
    await addRoomCategory(roomCategory);
    await reload();
  };

  const save = (roomCategory: RoomCategory) => attempt(async () => {
    await updateRoomCategory(roomCategory);
    await reload();
  });

  const remove = (name: string) => setAsking({
    name,
    confirmation: {
      caption: t('roomCategories.confirmDeleteCaption'),
      question: t('roomCategories.confirmDelete', { name }),
      onConfirm: () => attempt(async () => {
        await deleteRoomCategory(name);
        await reload();
      }),
    },
  });

  return (
    <section>
      <h2>{t('roomCategories.heading')}</h2>
      <ErrorMessage code={failure ?? loadFailure} />
      <table>
        <thead>
          <tr>
            <th>{t('roomCategories.roomCategory')}</th>
            <th>{t('roomCategories.description')}</th>
            <th />
          </tr>
        </thead>
        <tbody>
          {roomCategories.map(roomCategory => (
            <RoomCategoryRow
              key={roomCategory.name}
              roomCategory={roomCategory}
              asking={asking?.name === roomCategory.name}
              onSave={save}
              onDelete={remove}
            />
          ))}
        </tbody>
      </table>
      <AddRoomCategoryForm onAdd={add} />
      {asking && <ConfirmDialog {...asking.confirmation} onClose={() => setAsking(undefined)} />}
    </section>
  );
}

type Asking = { name: string; confirmation: Confirmation };
