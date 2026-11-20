import { useId, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorMessage } from '../ErrorMessage';
import type { RoomCategory } from '../roomCategories/roomCategoriesApi';
import { hasEmptyField, useEntered } from '../useEntered';
import { useFormAttempt } from '../useFormAttempt';
import type { Room } from './roomsApi';

type Props = { roomCategories: RoomCategory[]; bedCounts: number[]; onAdd: (room: Room) => Promise<void> };

type Entered = { roomNumber: string; roomCategory: string; bedCount?: number };

const fieldOf = {
  RoomAlreadyExists: 'roomNumber',
  RoomCategoryNotFound: 'roomCategory',
  InvalidBedCount: 'bedCount',
} as const;

const nothingEntered: Entered = { roomNumber: '', roomCategory: '' };

export function AddRoomForm({ roomCategories, bedCounts, onAdd }: Props) {
  const { t } = useTranslation();
  const { attempt, failureAt } = useFormAttempt(fieldOf);
  const { entered, enter, clear } = useEntered(nothingEntered);
  const room = roomOf(entered, bedCounts[0]);
  const roomNumberError = useId();
  const roomCategoryError = useId();
  const bedCountError = useId();
  const buttonError = useId();

  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (await attempt(() => onAdd(room)))
      clear();
  };

  return (
    <form onSubmit={submit}>
      <div className="field">
        <label>
          {t('rooms.roomNumber')}
          <input
            required
            value={room.roomNumber}
            onChange={event => enter({ roomNumber: event.target.value })}
            aria-describedby={roomNumberError}
          />
        </label>
        <ErrorMessage id={roomNumberError} code={failureAt('roomNumber')} />
      </div>
      <div className="field">
        <label>
          {t('rooms.roomCategory')}
          <select
            required
            value={room.roomCategory}
            onChange={event => enter({ roomCategory: event.target.value })}
            aria-describedby={roomCategoryError}
          >
            <option value="">{t('rooms.chooseRoomCategory')}</option>
            {roomCategories.map(category => <option key={category.name}>{category.name}</option>)}
          </select>
        </label>
        <ErrorMessage id={roomCategoryError} code={failureAt('roomCategory')} />
      </div>
      <div className="field">
        <label>
          {t('rooms.beds')}
          <select
            required
            value={room.bedCount}
            onChange={event => enter({ bedCount: Number(event.target.value) })}
            aria-describedby={bedCountError}
          >
            {bedCounts.map(count => <option key={count}>{count}</option>)}
          </select>
        </label>
        <ErrorMessage id={bedCountError} code={failureAt('bedCount')} />
      </div>
      <div className="field actions">
        <button disabled={hasEmptyField(room)} aria-describedby={buttonError}>{t('rooms.add')}</button>
        <ErrorMessage id={buttonError} code={failureAt('button')} />
      </div>
    </form>
  );
}

const roomOf = ({ roomNumber, roomCategory, bedCount }: Entered, fewestBeds: number): Room => ({
  roomNumber,
  bedCount: bedCount ?? fewestBeds,
  roomCategory,
});
