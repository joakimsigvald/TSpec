import { useId, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorMessage } from '../ErrorMessage';
import { useAttempt } from '../useAttempt';
import type { RoomCategory } from './roomCategoriesApi';

type Props = {
  roomCategory: RoomCategory;
  onSave: (roomCategory: RoomCategory) => Promise<void>;
  onClose: () => void;
};

export function ChangeRoomCategoryRow({ roomCategory, onSave, onClose }: Props) {
  const { t } = useTranslation();
  const { failure, attempt } = useAttempt();
  const [description, setDescription] = useState(roomCategory.description);
  const unchanged = description === roomCategory.description;
  const buttonError = useId();

  const save = async () => {
    if (await attempt(() => onSave({ name: roomCategory.name, description })))
      onClose();
  };

  return (
    <tr className="changing">
      <td>{roomCategory.name}</td>
      <td>
        <input
          required
          aria-label={t('roomCategories.descriptionOf', { name: roomCategory.name })}
          value={description}
          onChange={event => setDescription(event.target.value)}
        />
      </td>
      <td>
        <button disabled={unchanged || description === ''} onClick={save} aria-describedby={buttonError}>
          {t('general.save')}
        </button>
        <button onClick={onClose}>{t('general.cancelChange')}</button>
        <ErrorMessage id={buttonError} code={failure} />
      </td>
    </tr>
  );
}
