import { QueryClient, useQuery, useQueryClient } from '@tanstack/react-query';
import { listBookings } from './bookings/bookingsApi';
import { listRooms } from './rooms/roomsApi';
import { codeOf } from './useAttempt';

const rooms = ['rooms'];
const bookings = ['bookings'];

// A room states whether it has open bookings, so a booking change makes the rooms stale too.
const staleAfter = { room: [rooms], booking: [bookings, rooms] };

// A refusal is the API's answer, so asking again would only delay it.
export const newQueryClient = () => new QueryClient({ defaultOptions: { queries: { retry: false } } });

export function useRooms() {
  const { data, error } = useQuery({ queryKey: rooms, queryFn: listRooms });
  return { rooms: data ?? [], failure: error ? codeOf(error) : undefined };
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
