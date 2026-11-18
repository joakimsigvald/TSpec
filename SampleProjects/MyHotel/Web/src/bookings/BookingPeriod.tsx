import { useId, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorMessage } from '../ErrorMessage';
import { ShortcutButton } from './ShortcutButton';

type Props = { toFailure?: string };

type ToChoice = { typed: string } | { nights: number };

const nightChoices = [1, 2, 3];

export function BookingPeriod({ toFailure }: Props) {
  const { t } = useTranslation();
  const [from, setFrom] = useState('');
  const [toChoice, setToChoice] = useState<ToChoice>({ typed: '' });
  const toError = useId();
  const today = isoDate(new Date());
  const tomorrow = daysAfter(today, 1);
  const nightsAfterFrom = (nights: number) => (from ? daysAfter(from, nights) : undefined);
  const to = 'nights' in toChoice ? nightsAfterFrom(toChoice.nights) ?? '' : toChoice.typed;

  const chooseNights = (nights: number) => {
    setFrom(current => current || today);
    setToChoice({ nights });
  };

  return (
    <>
      <div className="field">
        <label>
          {t('bookings.from')}
          <input name="from" type="date" required value={from} onChange={event => setFrom(event.target.value)} />
        </label>
        <div className="shortcuts">
          <ShortcutButton pressed={from === today} onClick={() => setFrom(today)}>
            {t('bookings.today')}
          </ShortcutButton>
          <ShortcutButton pressed={from === tomorrow} onClick={() => setFrom(tomorrow)}>
            {t('bookings.tomorrow')}
          </ShortcutButton>
        </div>
      </div>
      <div className="field">
        <label>
          {t('bookings.to')}
          <input
            name="to"
            type="date"
            required
            value={to}
            onChange={event => setToChoice({ typed: event.target.value })}
            aria-describedby={toError}
          />
        </label>
        <div className="shortcuts">
          {nightChoices.map(nights => (
            <ShortcutButton key={nights} pressed={to === nightsAfterFrom(nights)} onClick={() => chooseNights(nights)}>
              {t('bookings.nights', { count: nights })}
            </ShortcutButton>
          ))}
        </div>
        <ErrorMessage id={toError} code={toFailure} />
      </div>
    </>
  );
}

function daysAfter(date: string, days: number) {
  const [year, month, day] = date.split('-').map(Number);
  return isoDate(new Date(year, month - 1, day + days));
}

const isoDate = (date: Date) => [date.getFullYear(), date.getMonth() + 1, date.getDate()].map(twoDigits).join('-');

const twoDigits = (part: number) => String(part).padStart(2, '0');
