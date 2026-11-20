import { useId, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorMessage } from '../ErrorMessage';
import { hasEmptyField, useEntered } from '../useEntered';
import { useFormAttempt } from '../useFormAttempt';
import type { RoomCategory } from './roomCategoriesApi';

type Props = { onAdd: (roomCategory: RoomCategory) => Promise<void> };

const fieldOf = { RoomCategoryAlreadyExists: 'name' } as const;

const nothingEntered: RoomCategory = { name: '', description: '' };

export function AddRoomCategoryForm({ onAdd }: Props) {
  const { t } = useTranslation();
  const { attempt, failureAt } = useFormAttempt(fieldOf);
  const { entered, enter, clear } = useEntered(nothingEntered);
  const nameError = useId();
  const buttonError = useId();

  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (await attempt(() => onAdd(entered)))
      clear();
  };

  return (
    <form onSubmit={submit}>
      <div className="field">
        <label>
          {t('roomCategories.name')}
          <input
            required
            value={entered.name}
            onChange={event => enter({ name: event.target.value })}
            aria-describedby={nameError}
          />
        </label>
        <ErrorMessage id={nameError} code={failureAt('name')} />
      </div>
      <div className="field">
        <label>
          {t('roomCategories.description')}
          <input required value={entered.description} onChange={event => enter({ description: event.target.value })} />
        </label>
      </div>
      <div className="field actions">
        <button disabled={hasEmptyField(entered)} aria-describedby={buttonError}>{t('roomCategories.add')}</button>
        <ErrorMessage id={buttonError} code={failureAt('button')} />
      </div>
    </form>
  );
}
