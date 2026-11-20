import { useTranslation } from 'react-i18next';
import type { Booking, BookingAction, BookingStatus } from './bookingsApi';

type Props = { booking: Booking; onAction: (action: BookingAction, bookingNumber: number) => void };

export function BookingRow({ booking: { bookingNumber, info, period, state }, onAction }: Props) {
  const { t } = useTranslation();
  const statusLabel = statusLabels[state.status];

  return (
    <tr>
      <td>{bookingNumber}</td>
      <td>{info.roomNumber}</td>
      <td>{info.guestCount}</td>
      <td>{info.guestName}</td>
      <td>{period.from}</td>
      <td>{period.to}</td>
      <td>
        {statusLabel && <span className="status">{t(statusLabel)}</span>}
        {state.actions.map(action => (
          <button key={action} onClick={() => onAction(action, bookingNumber)}>{t(actionLabels[action])}</button>
        ))}
      </td>
    </tr>
  );
}

const actionLabels: Record<BookingAction, string> = {
  Cancel: 'bookings.cancel',
  CheckIn: 'bookings.checkIn',
  CheckOut: 'bookings.checkOut',
};

// An open or past booking shows no status: an open one shows what can be done with it instead.
const statusLabels: Partial<Record<BookingStatus, string>> = {
  CheckedIn: 'bookings.checkedIn',
  CheckedOut: 'bookings.checkedOut',
  NoShow: 'bookings.noShow',
  Cancelled: 'bookings.cancelled',
};
