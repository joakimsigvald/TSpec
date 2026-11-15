import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';
import { fakeApi } from '../test/fakeApi';
import { Bookings } from './Bookings';

const request = { roomNumber: '101', guestName: 'Smith', from: '2026-08-10', to: '2026-08-12' };
const booking10001 = { bookingNumber: 10001, ...request };
const booking10002 = { bookingNumber: 10002, roomNumber: '102', guestName: 'Jones', from: '2026-08-11', to: '2026-08-13' };

const rowOf = (bookingNumber: number) => screen.findByRole('row', { name: new RegExp(`^${bookingNumber}`) });

const fillIn = async () => {
  await userEvent.type(screen.getByLabelText('Room number'), request.roomNumber);
  await userEvent.type(screen.getByLabelText('Guest'), request.guestName);
  await userEvent.type(screen.getByLabelText('From'), request.from);
  await userEvent.type(screen.getByLabelText('To'), request.to);
};

const bookRefusedWith = async (status: number, code: string) => {
  fakeApi({
    'GET /bookings': { status: 200, body: [] },
    'POST /bookings': { status, body: { code } },
  });
  render(<Bookings />);
  await fillIn();
  await userEvent.click(screen.getByRole('button', { name: 'Book' }));
};

describe('Bookings', () => {
  it('lists the bookings', async () => {
    fakeApi({ 'GET /bookings': { status: 200, body: [booking10001, booking10002] } });
    render(<Bookings />);
    expect(await rowOf(10001)).toHaveTextContent('10001101Smith2026-08-102026-08-12');
    expect(await rowOf(10002)).toHaveTextContent('10002102Jones2026-08-112026-08-13');
  });

  it('books the room entered', async () => {
    const api = fakeApi({
      'GET /bookings': { status: 200, body: [] },
      'POST /bookings': { status: 201, body: booking10001 },
    });
    render(<Bookings />);
    await fillIn();
    await userEvent.click(screen.getByRole('button', { name: 'Book' }));
    expect(await rowOf(10001)).toBeInTheDocument();
    expect(api).toHaveBeenCalledWith('/bookings', expect.objectContaining({
      method: 'POST',
      body: JSON.stringify(request),
    }));
  });

  it('says under the room number that there is no such room', async () => {
    await bookRefusedWith(404, 'RoomNotFound');
    await waitFor(() =>
      expect(screen.getByLabelText('Room number')).toHaveAccessibleDescription('Rummet finns inte.'));
  });

  it('says under the departure date that the period is too short', async () => {
    await bookRefusedWith(400, 'InvalidBookingPeriod');
    await waitFor(() =>
      expect(screen.getByLabelText('To')).toHaveAccessibleDescription('En bokning måste gälla minst en natt.'));
  });

  it('says under the room number that the room is already booked', async () => {
    await bookRefusedWith(409, 'RoomAlreadyBooked');
    await waitFor(() =>
      expect(screen.getByLabelText('Room number'))
        .toHaveAccessibleDescription('Rummet är redan bokat under några av nätterna.'));
  });

  it('says under the button why a booking could not be made for any other reason', async () => {
    await bookRefusedWith(500, 'Unknown');
    await waitFor(() =>
      expect(screen.getByRole('button', { name: 'Book' })).toHaveAccessibleDescription('Något gick fel.'));
  });

  it('removes a cancelled booking from the list', async () => {
    fakeApi({
      'GET /bookings': { status: 200, body: [booking10001, booking10002] },
      'DELETE /bookings/10001': { status: 204 },
    });
    render(<Bookings />);
    await userEvent.click(within(await rowOf(10001)).getByRole('button', { name: 'Cancel' }));
    await rowOf(10002);
    expect(screen.queryByRole('row', { name: /^10001/ })).not.toBeInTheDocument();
  });
});
