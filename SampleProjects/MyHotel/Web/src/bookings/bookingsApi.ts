import { get, post, postAction, put, remove } from '../http';

export type BookingStatus = 'Open' | 'CheckedIn' | 'CheckedOut' | 'NoShow' | 'Past' | 'Cancelled';

export type BookingAction = 'Cancel' | 'CheckIn' | 'CheckOut' | 'Pay';

export type BookingInfo = { roomNumber: string; guestName: string; guestCount: number };

export type BookingPeriod = { from: string; to: string };

export type BookingState = { status: BookingStatus; actions: BookingAction[] };

export type BookingPayment = { price: number; paid: boolean };

export type Booking = {
  bookingNumber: number;
  info: BookingInfo;
  period: BookingPeriod;
  state: BookingState;
  payment: BookingPayment;
};

export type BookingRequest = { info: BookingInfo; period: BookingPeriod };

export const listBookings = () => get<Booking[]>('/bookings');

export const bookRoom = (request: BookingRequest) => post<Booking>('/bookings', request);

export const changeBooking = (bookingNumber: number, request: BookingRequest) =>
  put<Booking>(`/bookings/${bookingNumber}`, request);

export const cancelBooking = (bookingNumber: number) => remove(`/bookings/${bookingNumber}`);

export const checkIn = (bookingNumber: number) => postAction(`/bookings/${bookingNumber}/check-in`);

export const checkOut = (bookingNumber: number) => postAction(`/bookings/${bookingNumber}/check-out`);

export const pay = (bookingNumber: number) => postAction(`/bookings/${bookingNumber}/pay`);

export const cancelRoomBookings = (roomNumber: string) => remove(`/rooms/${encodeURIComponent(roomNumber)}/bookings`);
