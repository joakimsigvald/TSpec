import { get, post, remove } from '../http';

export type BookingStatus = 'Open' | 'Past' | 'Cancelled';

export type BookingPeriod = { from: string; to: string };

export type Booking = {
  bookingNumber: number;
  roomNumber: string;
  guestName: string;
  guestCount: number;
  period: BookingPeriod;
  status: BookingStatus;
};

export type BookingRequest = Omit<Booking, 'bookingNumber' | 'status'>;

export const listBookings = () => get<Booking[]>('/bookings');

export const bookRoom = (request: BookingRequest) => post<Booking>('/bookings', request);

export const cancelBooking = (bookingNumber: number) => remove(`/bookings/${bookingNumber}`);

export const cancelOpenBookings = (roomNumber: string) =>
  remove(`/rooms/${encodeURIComponent(roomNumber)}/bookings`);
