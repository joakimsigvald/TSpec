import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { useState } from 'react';
import { describe, expect, it } from 'vitest';
import { fakeApi } from '../test/fakeApi';
import { Rooms } from './Rooms';
import type { Room } from './roomsApi';

function RoomsOnPage() {
  const [rooms, setRooms] = useState<Room[]>([]);
  return <Rooms rooms={rooms} onChange={setRooms} />;
}

const room101 = { roomNumber: '101', bedCount: 2 };
const room102 = { roomNumber: '102', bedCount: 1 };

const rowOf = (roomNumber: string) => screen.findByRole('row', { name: new RegExp(`^${roomNumber}`) });
const bedsIn = async (roomNumber: string) => within(await rowOf(roomNumber)).getByRole('spinbutton');

const addRoom = async (roomNumber: string, beds: string) => {
  await userEvent.type(screen.getByLabelText('Rumsnummer'), roomNumber);
  await userEvent.type(screen.getByLabelText('Sängar'), beds);
  await userEvent.click(screen.getByRole('button', { name: 'Lägg till rum' }));
};

describe('Rooms', () => {
  it('lists the rooms with their beds', async () => {
    fakeApi({ 'GET /rooms': { status: 200, body: [room101, room102] } });
    render(<RoomsOnPage />);
    expect(await bedsIn('101')).toHaveValue(2);
    expect(await bedsIn('102')).toHaveValue(1);
  });

  it('adds the room entered', async () => {
    const api = fakeApi({
      'GET /rooms': { status: 200, body: [room101] },
      'POST /rooms': { status: 201, body: { roomNumber: '103', bedCount: 3 } },
    });
    render(<RoomsOnPage />);
    await addRoom('103', '3');
    expect(await bedsIn('103')).toHaveValue(3);
    expect(api).toHaveBeenCalledWith('/rooms', expect.objectContaining({
      method: 'POST',
      body: JSON.stringify({ roomNumber: '103', bedCount: 3 }),
    }));
  });

  it('says under the room number that the room already exists', async () => {
    fakeApi({
      'GET /rooms': { status: 200, body: [room101] },
      'POST /rooms': { status: 409, body: { code: 'RoomAlreadyExists' } },
    });
    render(<RoomsOnPage />);
    await addRoom('101', '2');
    await waitFor(() =>
      expect(screen.getByLabelText('Rumsnummer')).toHaveAccessibleDescription('Rummet finns redan.'));
  });

  it('says under the button why a room could not be added for any other reason', async () => {
    fakeApi({
      'GET /rooms': { status: 200, body: [room101] },
      'POST /rooms': { status: 500 },
    });
    render(<RoomsOnPage />);
    await addRoom('103', '3');
    await waitFor(() =>
      expect(screen.getByRole('button', { name: 'Lägg till rum' })).toHaveAccessibleDescription('Något gick fel.'));
  });

  it('says something went wrong when the API gives no reason', async () => {
    fakeApi({ 'GET /rooms': { status: 500 } });
    render(<RoomsOnPage />);
    expect(await screen.findByRole('alert')).toHaveTextContent('Något gick fel.');
  });

  it('saves the beds entered for a room', async () => {
    const api = fakeApi({
      'GET /rooms': { status: 200, body: [room101] },
      'PUT /rooms/101': { status: 200, body: { roomNumber: '101', bedCount: 3 } },
    });
    render(<RoomsOnPage />);
    const beds = await bedsIn('101');
    await userEvent.clear(beds);
    await userEvent.type(beds, '3');
    await userEvent.click(within(await rowOf('101')).getByRole('button', { name: 'Spara' }));
    expect(api).toHaveBeenCalledWith('/rooms/101', expect.objectContaining({
      method: 'PUT',
      body: JSON.stringify({ roomNumber: '101', bedCount: 3 }),
    }));
  });

  it('removes a deleted room from the list', async () => {
    fakeApi({
      'GET /rooms': { status: 200, body: [room101, room102] },
      'DELETE /rooms/101': { status: 204 },
    });
    render(<RoomsOnPage />);
    await userEvent.click(within(await rowOf('101')).getByRole('button', { name: 'Ta bort' }));
    await rowOf('102');
    expect(screen.queryByRole('row', { name: /^101/ })).not.toBeInTheDocument();
  });
});
