import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';
import { App } from './App';
import { fakeApi } from './test/fakeApi';

const room101 = { roomNumber: '101', bedCount: 2 };
const room102 = { roomNumber: '102', bedCount: 1 };

const roomChoice = () => screen.getByRole('combobox', { name: 'Rumsnummer' });
const offered = (roomNumber: string) => within(roomChoice()).queryByRole('option', { name: roomNumber });

describe('App', () => {
  it('offers a room for booking as soon as it is added', async () => {
    fakeApi({
      'GET /rooms': { status: 200, body: [room101] },
      'GET /bookings': { status: 200, body: [] },
      'POST /rooms': { status: 201, body: room102 },
    });
    render(<App />);
    await userEvent.type(screen.getByRole('textbox', { name: 'Rumsnummer' }), room102.roomNumber);
    await userEvent.type(screen.getByLabelText('Sängar'), String(room102.bedCount));
    await userEvent.click(screen.getByRole('button', { name: 'Lägg till rum' }));
    await waitFor(() => expect(offered('102')).toBeInTheDocument());
  });

  it('stops offering a room for booking as soon as it is deleted', async () => {
    fakeApi({
      'GET /rooms': { status: 200, body: [room101, room102] },
      'GET /bookings': { status: 200, body: [] },
      'DELETE /rooms/101': { status: 204 },
    });
    render(<App />);
    const row101 = await screen.findByRole('row', { name: /^101/ });
    await userEvent.click(within(row101).getByRole('button', { name: 'Ta bort' }));
    await waitFor(() => expect(offered('101')).not.toBeInTheDocument());
    expect(offered('102')).toBeInTheDocument();
  });
});
