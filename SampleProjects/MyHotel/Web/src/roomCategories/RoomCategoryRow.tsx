import { useId, useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import type { RoomCategory, RoomCategoryOverview } from './roomCategoriesApi';

type Props = {
  roomCategory: RoomCategoryOverview;
  asking: boolean;
  onSave: (roomCategory: RoomCategory) => void;
  onDelete: (name: string) => void;
};

// A form cannot span table cells, so the description joins the buttons' form by its id: an empty
// description is then stopped there, as in the form that adds a category.
export function RoomCategoryRow({ roomCategory, asking, onSave, onDelete }: Props) {
  const { t } = useTranslation();
  const [description, setDescription] = useState(roomCategory.description);
  const form = useId();

  const submit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    onSave({ name: roomCategory.name, description });
  };

  return (
    <tr className={asking ? 'asking' : undefined}>
      <td>{roomCategory.name}</td>
      <td>
        <input
          form={form}
          required
          aria-label={t('roomCategories.descriptionOf', { name: roomCategory.name })}
          value={description}
          onChange={event => setDescription(event.target.value)}
        />
      </td>
      <td>
        <form id={form} onSubmit={submit}>
          <button>{t('roomCategories.save')}</button>
          <button
            type="button"
            disabled={roomCategory.hasRooms}
            title={roomCategory.hasRooms ? t('errors.RoomCategoryHasRooms') : undefined}
            onClick={() => onDelete(roomCategory.name)}
          >
            {t('roomCategories.delete')}
          </button>
        </form>
      </td>
    </tr>
  );
}
