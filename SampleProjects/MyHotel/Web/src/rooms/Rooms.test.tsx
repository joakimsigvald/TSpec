import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';
import { fakeApi } from '../test/fakeApi';
import { Rooms } from './Rooms';

const room101 = { roomNumber: '101', bedCount: 2 };
const room102 = { roomNumber: '102', bedCount: 1 };

const rowOf = (roomNumber: string) => screen.findByRole('row', { name: new RegExp(`^${roomNumber}`) });
const bedsIn = async (roomNumber: string) => within(await rowOf(roomNumber)).getByRole('spinbutton');

const addRoom = async (roomNumber: string, beds: string) => {
  await userEvent.type(screen.getByLabelText('Room number'), roomNumber);
  await userEvent.type(screen.getByLabelText('Beds'), beds);
  await userEvent.click(screen.getByRole('button', { name: 'Add room' }));
};

describe('Rooms', () => {
  it('lists the rooms with their beds', async () => {
    fakeApi({ 'GET /rooms': { status: 200, body: [room101, room102] } });
    render(<Rooms />);
    expect(await bedsIn('101')).toHaveValue(2);
    expect(await bedsIn('102')).toHaveValue(1);
  });

  it('adds the room entered', async () => {
    const api = fakeApi({
      'GET /rooms': { status: 200, body: [room101] },
      'POST /rooms': { status: 201, body: { roomNumber: '103', bedCount: 3 } },
    });
    render(<Rooms />);
    await addRoom('103', '3');
    expect(await bedsIn('103')).toHaveValue(3);
    expect(api).toHaveBeenCalledWith('/rooms', expect.objectContaining({
      method: 'POST',
      body: JSON.stringify({ roomNumber: '103', bedCount: 3 }),
    }));
  });

  it('says why a room could not be added', async () => {
    fakeApi({
      'GET /rooms': { status: 200, body: [room101] },
      'POST /rooms': { status: 409, body: { error: 'Room 101 already exists.' } },
    });
    render(<Rooms />);
    await addRoom('101', '2');
    expect(await screen.findByRole('alert')).toHaveTextContent('Room 101 already exists.');
  });

  it('saves the beds entered for a room', async () => {
    const api = fakeApi({
      'GET /rooms': { status: 200, body: [room101] },
      'PUT /rooms/101': { status: 200, body: { roomNumber: '101', bedCount: 3 } },
    });
    render(<Rooms />);
    const beds = await bedsIn('101');
    await userEvent.clear(beds);
    await userEvent.type(beds, '3');
    await userEvent.click(within(await rowOf('101')).getByRole('button', { name: 'Save' }));
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
    render(<Rooms />);
    await userEvent.click(within(await rowOf('101')).getByRole('button', { name: 'Delete' }));
    await rowOf('102');
    expect(screen.queryByRole('row', { name: /^101/ })).not.toBeInTheDocument();
  });
});
