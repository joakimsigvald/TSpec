import { useId, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorMessage } from '../ErrorMessage';
import type { RoomCategory } from '../roomCategories/roomCategoriesApi';
import { useFormAttempt } from '../useFormAttempt';
import type { Room } from './roomsApi';

type Props = { roomCategories: RoomCategory[]; onAdd: (room: Room) => Promise<void> };

const fieldOf = { RoomAlreadyExists: 'roomNumber', RoomCategoryNotFound: 'roomCategory' } as const;

export function AddRoomForm({ roomCategories, onAdd }: Props) {
  const { t } = useTranslation();
  const { attempt, failureAt } = useFormAttempt(fieldOf);
  const roomNumberError = useId();
  const roomCategoryError = useId();
  const buttonError = useId();

  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = event.currentTarget;
    if (await attempt(() => onAdd(roomIn(new FormData(form)))))
      form.reset();
  };

  return (
    <form onSubmit={submit}>
      <div className="field">
        <label>
          {t('rooms.roomNumber')}
          <input name="roomNumber" required aria-describedby={roomNumberError} />
        </label>
        <ErrorMessage id={roomNumberError} code={failureAt('roomNumber')} />
      </div>
      <div className="field">
        <label>
          {t('rooms.roomCategory')}
          <select name="roomCategory" required aria-describedby={roomCategoryError}>
            <option value="">{t('rooms.chooseRoomCategory')}</option>
            {roomCategories.map(category => <option key={category.name}>{category.name}</option>)}
          </select>
        </label>
        <ErrorMessage id={roomCategoryError} code={failureAt('roomCategory')} />
      </div>
      <div className="field">
        <label>
          {t('rooms.beds')}
          <input name="bedCount" type="number" required />
        </label>
      </div>
      <div className="field actions">
        <button aria-describedby={buttonError}>{t('rooms.add')}</button>
        <ErrorMessage id={buttonError} code={failureAt('button')} />
      </div>
    </form>
  );
}

const roomIn = (data: FormData): Room => ({
  roomNumber: String(data.get('roomNumber')),
  bedCount: Number(data.get('bedCount')),
  roomCategory: String(data.get('roomCategory')),
});
