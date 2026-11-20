import type { Room } from '../rooms/roomsApi';
import type { BookingRequest } from './bookingsApi';

const entryFields = ['from', 'to', 'roomNumber', 'guestCount', 'guestName'] as const;

export type BookingEntry = Record<(typeof entryFields)[number], string>;

export const fieldOf = {
  RoomNotFound: 'roomNumber',
  RoomAlreadyBooked: 'roomNumber',
  InvalidGuestCount: 'guestCount',
  TooManyGuests: 'guestCount',
  InvalidBookingPeriod: 'to',
} as const;

export const inAlphanumericOrder = (roomNumbers: string[]) =>
  [...roomNumbers].sort((one, other) => one.localeCompare(other, 'sv', { numeric: true }));

export const roomNumbersOf = (rooms: Room[]) => rooms.map(room => room.roomNumber);

export const requestOf = ({ from, to, roomNumber, guestCount, guestName }: BookingEntry): BookingRequest => ({
  roomNumber,
  guestName,
  guestCount: Number(guestCount),
  period: { from, to },
});

export const sameEntry = (one: BookingEntry, other: BookingEntry) =>
  entryFields.every(field => one[field] === other[field]);
