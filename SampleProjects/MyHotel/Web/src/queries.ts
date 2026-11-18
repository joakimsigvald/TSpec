import { QueryClient, useQuery, useQueryClient } from '@tanstack/react-query';
import { listBookings } from './bookings/bookingsApi';
import { listRoomCategories, type RoomCategoryOverview } from './roomCategories/roomCategoriesApi';
import { listRooms } from './rooms/roomsApi';
import { codeOf } from './useAttempt';

const rooms = ['rooms'];
const roomCategories = ['roomCategories'];
const bookings = ['bookings'];

// A room states whether it has open bookings, and a category whether a room has it, so a change
// makes those lists stale too.
const staleAfter = { room: [rooms, roomCategories], roomCategory: [roomCategories], booking: [bookings, rooms] };

// A refusal is the API's answer, so asking again would only delay it.
export const newQueryClient = () => new QueryClient({ defaultOptions: { queries: { retry: false } } });

export function useRooms() {
  const { data, error } = useQuery({ queryKey: rooms, queryFn: listRooms });
  return { rooms: data ?? [], failure: error ? codeOf(error) : undefined };
}

export function useRoomCategories() {
  const { data, error } = useQuery({ queryKey: roomCategories, queryFn: listRoomCategories, select: inNameOrder });
  return { roomCategories: data ?? [], failure: error ? codeOf(error) : undefined };
}

export function useBookings() {
  const { data, error } = useQuery({ queryKey: bookings, queryFn: listBookings });
  return { bookings: data ?? [], failure: error ? codeOf(error) : undefined };
}

export function useReload(change: keyof typeof staleAfter) {
  const client = useQueryClient();
  return async () => {
    await Promise.all(staleAfter[change].map(queryKey => client.invalidateQueries({ queryKey })));
  };
}

// Swedish order, Å before Ä before Ö, which the API's exact comparison of names does not give.
const inNameOrder = (categories: RoomCategoryOverview[]) =>
  [...categories].sort((one, other) => one.name.localeCompare(other.name, 'sv'));
