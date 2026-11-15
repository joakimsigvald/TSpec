import { get, post, remove } from '../http';

export type Booking = {
  bookingNumber: number;
  roomNumber: string;
  guestName: string;
  from: string;
  to: string;
};

export type BookingRequest = Omit<Booking, 'bookingNumber'>;

export const listBookings = () => get<Booking[]>('/bookings');

export const bookRoom = (request: BookingRequest) => post<Booking>('/bookings', request);

export const cancelBooking = (bookingNumber: number) => remove(`/bookings/${bookingNumber}`);
