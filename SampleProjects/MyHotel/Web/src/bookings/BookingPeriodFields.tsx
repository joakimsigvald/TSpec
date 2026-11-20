import { useId, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorMessage } from '../ErrorMessage';
import type { BookingPeriod } from './bookingsApi';
import { ShortcutButton } from './ShortcutButton';

type Props = { period: BookingPeriod; onChange: (period: BookingPeriod) => void; toFailure?: string };

const nightChoices = [1, 2, 3];

export function BookingPeriodFields({ period: { from, to }, onChange, toFailure }: Props) {
  const { t } = useTranslation();
  const [nightsChosen, setNightsChosen] = useState<number>();
  const toError = useId();
  const today = isoDate(new Date());
  const tomorrow = daysAfter(today, 1);

  const arrive = (arrival: string) =>
    onChange({ from: arrival, to: nightsChosen === undefined ? to : nightsAfter(arrival, nightsChosen) ?? '' });

  const depart = (departure: string) => {
    setNightsChosen(undefined);
    onChange({ from, to: departure });
  };

  const chooseNights = (nights: number) => {
    const arrival = from || today;
    setNightsChosen(nights);
    onChange({ from: arrival, to: daysAfter(arrival, nights) });
  };

  return (
    <>
      <div className="field">
        <label>
          {t('bookings.from')}
          <input type="date" required value={from} onChange={event => arrive(event.target.value)} />
        </label>
        <div className="shortcuts">
          <ShortcutButton pressed={from === today} onClick={() => arrive(today)}>
            {t('bookings.today')}
          </ShortcutButton>
          <ShortcutButton pressed={from === tomorrow} onClick={() => arrive(tomorrow)}>
            {t('bookings.tomorrow')}
          </ShortcutButton>
        </div>
      </div>
      <div className="field">
        <label>
          {t('bookings.to')}
          <input
            type="date"
            required
            value={to}
            onChange={event => depart(event.target.value)}
            aria-describedby={toError}
          />
        </label>
        <div className="shortcuts">
          {nightChoices.map(nights => (
            <ShortcutButton key={nights} pressed={to === nightsAfter(from, nights)} onClick={() => chooseNights(nights)}>
              {t('bookings.nights', { count: nights })}
            </ShortcutButton>
          ))}
        </div>
        <ErrorMessage id={toError} code={toFailure} />
      </div>
    </>
  );
}

const nightsAfter = (arrival: string, nights: number) => (arrival ? daysAfter(arrival, nights) : undefined);

function daysAfter(date: string, days: number) {
  const [year, month, day] = date.split('-').map(Number);
  return isoDate(new Date(year, month - 1, day + days));
}

const isoDate = (date: Date) => [date.getFullYear(), date.getMonth() + 1, date.getDate()].map(twoDigits).join('-');

const twoDigits = (part: number) => String(part).padStart(2, '0');
