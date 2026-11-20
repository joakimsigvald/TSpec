import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import type { Room } from '../rooms/roomsApi';
import type { Booking, BookingAction, BookingRequest, BookingStatus } from './bookingsApi';
import { ChangeBookingRow } from './ChangeBookingRow';

type Props = {
  booking: Booking;
  rooms: Room[];
  onAction: (action: BookingAction, bookingNumber: number) => void;
  onSave: (bookingNumber: number, request: BookingRequest) => Promise<void>;
};

export function BookingRow({ booking, rooms, onAction, onSave }: Props) {
  const { t } = useTranslation();
  const [changing, setChanging] = useState(false);
  const { bookingNumber, info, period, state } = booking;
  const statusLabel = statusLabels[state.status];

  if (changing)
    return (
      <ChangeBookingRow
        booking={booking}
        rooms={rooms}
        onSave={request => onSave(bookingNumber, request)}
        onClose={() => setChanging(false)}
      />
    );

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
        <button onClick={() => setChanging(true)}>{t('bookings.change')}</button>
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
