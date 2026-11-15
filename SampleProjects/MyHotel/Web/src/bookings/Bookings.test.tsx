import { render, screen, within } from '@testing-library/react';
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

  it('says why a booking was refused', async () => {
    fakeApi({
      'GET /bookings': { status: 200, body: [booking10001] },
      'POST /bookings': { status: 409, body: { error: 'Room 101 is already booked.' } },
    });
    render(<Bookings />);
    await fillIn();
    await userEvent.click(screen.getByRole('button', { name: 'Book' }));
    expect(await screen.findByRole('alert')).toHaveTextContent('Room 101 is already booked.');
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
