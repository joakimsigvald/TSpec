import { useId, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorMessage } from '../ErrorMessage';
import { useFormAttempt } from '../useFormAttempt';
import type { RoomCategory } from './roomCategoriesApi';

type Props = { onAdd: (roomCategory: RoomCategory) => Promise<void> };

const fieldOf = { RoomCategoryAlreadyExists: 'name' } as const;

export function AddRoomCategoryForm({ onAdd }: Props) {
  const { t } = useTranslation();
  const { attempt, failureAt } = useFormAttempt(fieldOf);
  const nameError = useId();
  const buttonError = useId();

  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = event.currentTarget;
    if (await attempt(() => onAdd(roomCategoryIn(new FormData(form)))))
      form.reset();
  };

  return (
    <form onSubmit={submit}>
      <div className="field">
        <label>
          {t('roomCategories.name')}
          <input name="name" required aria-describedby={nameError} />
        </label>
        <ErrorMessage id={nameError} code={failureAt('name')} />
      </div>
      <div className="field">
        <label>
          {t('roomCategories.description')}
          <input name="description" required />
        </label>
      </div>
      <div className="field actions">
        <button aria-describedby={buttonError}>{t('roomCategories.add')}</button>
        <ErrorMessage id={buttonError} code={failureAt('button')} />
      </div>
    </form>
  );
}

const roomCategoryIn = (data: FormData): RoomCategory => ({
  name: String(data.get('name')),
  description: String(data.get('description')),
});
