import { screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';
import { fakeApi } from '../test/fakeApi';
import { renderPage } from '../test/renderPage';
import { Bookings } from './Bookings';

const request = { roomNumber: '101', guestName: 'Smith', from: '2026-08-10', to: '2026-08-12' };
const booking10001 = { bookingNumber: 10001, ...request, status: 'Open' };
const booking10002 = {
  bookingNumber: 10002, roomNumber: '102', guestName: 'Jones', from: '2026-08-11', to: '2026-08-13', status: 'Open',
};
const rooms = ['101', '12A', '2', '12'].map(roomNumber => ({ roomNumber, bedCount: 1, hasOpenBookings: false }));
const roomsReply = { status: 200, body: rooms };

const rowOf = (bookingNumber: number) => screen.findByRole('row', { name: new RegExp(`^${bookingNumber}`) });

const fillIn = async () => {
  await userEvent.selectOptions(await screen.findByLabelText('Rumsnummer'), request.roomNumber);
  await userEvent.type(screen.getByLabelText('Gäst'), request.guestName);
  await userEvent.type(screen.getByLabelText('Från'), request.from);
  await userEvent.type(screen.getByLabelText('Till'), request.to);
};

const offeredRooms = () =>
  within(screen.getByLabelText('Rumsnummer')).getAllByRole('option').map(option => option.textContent);

const bookRefusedWith = async (status: number, code: string) => {
  fakeApi({
    'GET /rooms': roomsReply,
    'GET /bookings': { status: 200, body: [] },
    'POST /bookings': { status, body: { code } },
  });
  renderPage(<Bookings />);
  await waitFor(() => expect(offeredRooms()).toContain(request.roomNumber));
  await fillIn();
  await userEvent.click(screen.getByRole('button', { name: 'Boka' }));
};

const bookAccepted = () =>
  fakeApi({
    'GET /rooms': roomsReply,
    'GET /bookings': [{ status: 200, body: [] }, { status: 200, body: [booking10001] }],
    'POST /bookings': { status: 201, body: booking10001 },
  });

const book = async () => {
  renderPage(<Bookings />);
  await waitFor(() => expect(offeredRooms()).toContain(request.roomNumber));
  await fillIn();
  await userEvent.click(screen.getByRole('button', { name: 'Boka' }));
};

describe('Bookings', () => {
  it('lists the bookings', async () => {
    fakeApi({ 'GET /rooms': roomsReply, 'GET /bookings': { status: 200, body: [booking10001, booking10002] } });
    renderPage(<Bookings />);
    expect(await rowOf(10001)).toHaveTextContent('10001101Smith2026-08-102026-08-12');
    expect(await rowOf(10002)).toHaveTextContent('10002102Jones2026-08-112026-08-13');
  });

  it('offers the rooms in alphanumeric order', async () => {
    fakeApi({ 'GET /rooms': roomsReply, 'GET /bookings': { status: 200, body: [] } });
    renderPage(<Bookings />);
    await waitFor(() => expect(offeredRooms()).toEqual(['Välj rum', '2', '12', '12A', '101']));
  });

  it('books the room chosen', async () => {
    const api = bookAccepted();
    await book();
    expect(await rowOf(10001)).toBeInTheDocument();
    expect(api).toHaveBeenCalledWith('/bookings', expect.objectContaining({
      method: 'POST',
      body: JSON.stringify(request),
    }));
  });

  it('asks for a room again after a booking', async () => {
    bookAccepted();
    await book();
    await rowOf(10001);
    expect(screen.getByLabelText('Rumsnummer')).toHaveDisplayValue('Välj rum');
  });

  it('says under the room number that there is no such room', async () => {
    await bookRefusedWith(404, 'RoomNotFound');
    await waitFor(() =>
      expect(screen.getByLabelText('Rumsnummer')).toHaveAccessibleDescription('Rummet finns inte.'));
  });

  it('says under the departure date that the period is too short', async () => {
    await bookRefusedWith(400, 'InvalidBookingPeriod');
    await waitFor(() =>
      expect(screen.getByLabelText('Till')).toHaveAccessibleDescription('En bokning måste gälla minst en natt.'));
  });

  it('says under the room number that the room is already booked', async () => {
    await bookRefusedWith(409, 'RoomAlreadyBooked');
    await waitFor(() =>
      expect(screen.getByLabelText('Rumsnummer'))
        .toHaveAccessibleDescription('Rummet är redan bokat under några av nätterna.'));
  });

  it('says under the button why a booking could not be made for any other reason', async () => {
    await bookRefusedWith(500, 'Unknown');
    await waitFor(() =>
      expect(screen.getByRole('button', { name: 'Boka' })).toHaveAccessibleDescription('Något gick fel.'));
  });

  it('keeps a cancelled booking in the list, shown as cancelled', async () => {
    const api = fakeApi({
      'GET /rooms': roomsReply,
      'GET /bookings': [
        { status: 200, body: [booking10001, booking10002] },
        { status: 200, body: [{ ...booking10001, status: 'Cancelled' }, booking10002] },
      ],
      'DELETE /bookings/10001': { status: 204 },
    });
    renderPage(<Bookings />);
    await userEvent.click(within(await rowOf(10001)).getByRole('button', { name: 'Avboka' }));
    await waitFor(async () => expect(await rowOf(10001)).toHaveTextContent(/Avbokad$/));
    expect(api).toHaveBeenCalledWith('/bookings/10001', expect.objectContaining({ method: 'DELETE' }));
  });

  it('offers no cancelling of a cancelled booking', async () => {
    fakeApi({ 'GET /rooms': roomsReply, 'GET /bookings': { status: 200, body: [{ ...booking10001, status: 'Cancelled' }] } });
    renderPage(<Bookings />);
    expect(within(await rowOf(10001)).queryByRole('button', { name: 'Avboka' })).not.toBeInTheDocument();
  });

  it('offers no cancelling of a past booking, nor calls it cancelled', async () => {
    fakeApi({ 'GET /rooms': roomsReply, 'GET /bookings': { status: 200, body: [{ ...booking10001, status: 'Past' }] } });
    renderPage(<Bookings />);
    const row = await rowOf(10001);
    expect(within(row).queryByRole('button', { name: 'Avboka' })).not.toBeInTheDocument();
    expect(row).toHaveTextContent(/2026-08-12$/);
  });

  it('says above the bookings that a booking past or cancelled was not cancelled', async () => {
    fakeApi({
      'GET /rooms': roomsReply,
      'GET /bookings': { status: 200, body: [booking10001] },
      'DELETE /bookings/10001': { status: 409, body: { code: 'BookingNotOpen' } },
    });
    renderPage(<Bookings />);
    await userEvent.click(within(await rowOf(10001)).getByRole('button', { name: 'Avboka' }));
    expect(await screen.findByRole('alert')).toHaveTextContent('Bokningen är redan avbokad eller passerad.');
  });
});
