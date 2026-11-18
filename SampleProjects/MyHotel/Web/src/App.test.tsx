import { screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';
import { App } from './App';
import { fakeApi } from './test/fakeApi';
import { answer } from './test/answer';
import { renderPage } from './test/renderPage';

const room101 = { roomNumber: '101', bedCount: 2, hasOpenBookings: false };
const room102 = { roomNumber: '102', bedCount: 1, hasOpenBookings: false };
const booking10001 = {
  bookingNumber: 10001, roomNumber: '101', guestName: 'Smith', from: '2026-08-10', to: '2026-08-12', status: 'Open',
};

const roomChoice = () => screen.getByRole('combobox', { name: 'Rumsnummer' });
const offered = (roomNumber: string) => within(roomChoice()).queryByRole('option', { name: roomNumber });
const roomButton = async (roomNumber: string, name: string) =>
  within(await screen.findByRole('row', { name: new RegExp(`^${roomNumber}`) })).getByRole('button', { name });

describe('App', () => {
  it('offers a room for booking as soon as it is added', async () => {
    fakeApi({
      'GET /rooms': [{ status: 200, body: [room101] }, { status: 200, body: [room101, room102] }],
      'GET /bookings': { status: 200, body: [] },
      'POST /rooms': { status: 201, body: room102 },
    });
    renderPage(<App />);
    await userEvent.type(screen.getByRole('textbox', { name: 'Rumsnummer' }), room102.roomNumber);
    await userEvent.type(screen.getByLabelText('Sängar'), String(room102.bedCount));
    await userEvent.click(screen.getByRole('button', { name: 'Lägg till rum' }));
    await waitFor(() => expect(offered('102')).toBeInTheDocument());
  });

  it('stops offering a room for booking as soon as it is deleted', async () => {
    fakeApi({
      'GET /rooms': [{ status: 200, body: [room101, room102] }, { status: 200, body: [room102] }],
      'GET /bookings': { status: 200, body: [] },
      'DELETE /rooms/101': { status: 204 },
    });
    renderPage(<App />);
    const row101 = await screen.findByRole('row', { name: /^101/ });
    await userEvent.click(within(row101).getByRole('button', { name: 'Ta bort' }));
    await answer('Ta bort rum', 'OK');
    await waitFor(() => expect(offered('101')).not.toBeInTheDocument());
    expect(offered('102')).toBeInTheDocument();
  });

  it('stops a room being deleted as soon as it is booked', async () => {
    fakeApi({
      'GET /rooms': [{ status: 200, body: [room101] }, { status: 200, body: [{ ...room101, hasOpenBookings: true }] }],
      'GET /bookings': [{ status: 200, body: [] }, { status: 200, body: [booking10001] }],
      'POST /bookings': { status: 201, body: booking10001 },
    });
    renderPage(<App />);
    await waitFor(() => expect(offered('101')).toBeInTheDocument());
    await userEvent.selectOptions(roomChoice(), '101');
    await userEvent.type(screen.getByLabelText('Gäst'), booking10001.guestName);
    await userEvent.type(screen.getByLabelText('Från'), booking10001.from);
    await userEvent.type(screen.getByLabelText('Till'), booking10001.to);
    await userEvent.click(screen.getByRole('button', { name: 'Boka' }));
    await waitFor(async () => expect(await roomButton('101', 'Ta bort')).toBeDisabled());
  });

  it('lets a room be deleted as soon as its last open booking is cancelled', async () => {
    fakeApi({
      'GET /rooms': [{ status: 200, body: [{ ...room101, hasOpenBookings: true }] }, { status: 200, body: [room101] }],
      'GET /bookings': [
        { status: 200, body: [booking10001] },
        { status: 200, body: [{ ...booking10001, status: 'Cancelled' }] },
      ],
      'DELETE /bookings/10001': { status: 204 },
    });
    renderPage(<App />);
    const booking = await screen.findByRole('row', { name: /^10001/ });
    await userEvent.click(within(booking).getByRole('button', { name: 'Avboka' }));
    await waitFor(async () => expect(await roomButton('101', 'Ta bort')).toBeEnabled());
  });

  it("shows a room's bookings as cancelled as soon as they are cancelled", async () => {
    fakeApi({
      'GET /rooms': [{ status: 200, body: [{ ...room101, hasOpenBookings: true }] }, { status: 200, body: [room101] }],
      'GET /bookings': [
        { status: 200, body: [booking10001] },
        { status: 200, body: [{ ...booking10001, status: 'Cancelled' }] },
      ],
      'DELETE /rooms/101/bookings': { status: 204 },
    });
    renderPage(<App />);
    await userEvent.click(await roomButton('101', 'Avboka alla'));
    await answer('Avboka alla bokningar', 'OK');
    await waitFor(async () =>
      expect(await screen.findByRole('row', { name: /^10001/ })).toHaveTextContent(/Avbokad$/));
  });
});
