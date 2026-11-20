import { screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';
import { App } from './App';
import { fakeApi } from './test/fakeApi';
import { answer } from './test/answer';
import { renderPage } from './test/renderPage';
import type { Booking, BookingAction, BookingStatus } from './bookings/bookingsApi';

const unbooked = { held: false, hasCancellableBookings: false };
const room101 = { roomNumber: '101', bedCount: 2, roomCategory: 'Standard', ...unbooked };
const room102 = { roomNumber: '102', bedCount: 1, roomCategory: 'Standard', ...unbooked };
const withBookingsToCancel = { ...room101, held: true, hasCancellableBookings: true };
const withGuestCheckedIn = { ...room101, held: true };
const booking10001: Booking = {
  bookingNumber: 10001,
  info: { roomNumber: '101', guestName: 'Smith', guestCount: 2 },
  period: { from: '2026-08-10', to: '2026-08-12' },
  state: { status: 'Open', actions: ['Cancel'] },
};

const inState = (status: BookingStatus, actions: BookingAction[]): Booking =>
  ({ ...booking10001, state: { status, actions } });

const standard = { name: 'Standard', description: 'Standardrum', hasRooms: false };
const suite = { name: 'Svit', description: 'Två rum', hasRooms: false };

const fakeHotel = (replies: Parameters<typeof fakeApi>[0]) =>
  fakeApi({
    'GET /room-categories': { status: 200, body: [standard] },
    'GET /rules': { status: 200, body: { minBedCount: 1, maxBedCount: 9 } },
    ...replies,
  });

const bedsChoice = () => screen.getByRole('combobox', { name: 'Sängar' });
const chooseBeds = async (count: number) =>
  userEvent.selectOptions(bedsChoice(), await within(bedsChoice()).findByRole('option', { name: String(count) }));
const roomChoice = () => screen.getByRole('combobox', { name: 'Rumsnummer' });
const offered = (roomNumber: string) => within(roomChoice()).queryByRole('option', { name: roomNumber });
const categoryChoice = () => screen.getByRole('combobox', { name: 'Kategori' });
const categoryOffered = (name: string) => within(categoryChoice()).queryByRole('option', { name });
const rowOf = (name: string) => screen.findByRole('row', { name: new RegExp(`^${name}`) });
const roomButton = async (roomNumber: string, name: string) =>
  within(await rowOf(roomNumber)).getByRole('button', { name });

describe('App', () => {
  it('offers a room for booking as soon as it is added', async () => {
    fakeHotel({
      'GET /rooms': [{ status: 200, body: [room101] }, { status: 200, body: [room101, room102] }],
      'GET /bookings': { status: 200, body: [] },
      'POST /rooms': { status: 201, body: room102 },
    });
    renderPage(<App />);
    await userEvent.type(screen.getByRole('textbox', { name: 'Rumsnummer' }), room102.roomNumber);
    await chooseBeds(room102.bedCount);
    await waitFor(() => expect(categoryOffered('Standard')).toBeInTheDocument());
    await userEvent.selectOptions(categoryChoice(), 'Standard');
    await userEvent.click(screen.getByRole('button', { name: 'Lägg till rum' }));
    await waitFor(() => expect(offered('102')).toBeInTheDocument());
  });

  it('offers a category for a room as soon as it is added', async () => {
    fakeHotel({
      'GET /room-categories': [{ status: 200, body: [standard] }, { status: 200, body: [standard, suite] }],
      'GET /rooms': { status: 200, body: [] },
      'GET /bookings': { status: 200, body: [] },
      'POST /room-categories': { status: 201, body: suite },
    });
    renderPage(<App />);
    await userEvent.type(screen.getByLabelText('Namn'), suite.name);
    await userEvent.type(screen.getByLabelText('Beskrivning'), suite.description);
    await userEvent.click(screen.getByRole('button', { name: 'Lägg till kategori' }));
    await waitFor(() => expect(categoryOffered('Svit')).toBeInTheDocument());
  });

  it('stops offering a category for a room as soon as it is deleted', async () => {
    fakeHotel({
      'GET /room-categories': [{ status: 200, body: [standard, suite] }, { status: 200, body: [standard] }],
      'GET /rooms': { status: 200, body: [] },
      'GET /bookings': { status: 200, body: [] },
      'DELETE /room-categories/Svit': { status: 204 },
    });
    renderPage(<App />);
    await userEvent.click(within(await rowOf('Svit')).getByRole('button', { name: 'Ta bort' }));
    await answer('Ta bort kategori', 'OK');
    await waitFor(() => expect(categoryOffered('Svit')).not.toBeInTheDocument());
  });

  it('stops a category being deleted as soon as a room has it', async () => {
    fakeHotel({
      'GET /room-categories': [
        { status: 200, body: [standard, suite] },
        { status: 200, body: [standard, { ...suite, hasRooms: true }] },
      ],
      'GET /rooms': [{ status: 200, body: [] }, { status: 200, body: [{ ...room101, roomCategory: 'Svit' }] }],
      'GET /bookings': { status: 200, body: [] },
      'POST /rooms': { status: 201, body: { ...room101, roomCategory: 'Svit' } },
    });
    renderPage(<App />);
    await userEvent.type(screen.getByRole('textbox', { name: 'Rumsnummer' }), room101.roomNumber);
    await chooseBeds(room101.bedCount);
    await waitFor(() => expect(categoryOffered('Svit')).toBeInTheDocument());
    await userEvent.selectOptions(categoryChoice(), 'Svit');
    await userEvent.click(screen.getByRole('button', { name: 'Lägg till rum' }));
    await waitFor(async () =>
      expect(within(await rowOf('Svit')).getByRole('button', { name: 'Ta bort' })).toBeDisabled());
  });

  it('lets a category be deleted as soon as its last room moves to another', async () => {
    fakeHotel({
      'GET /room-categories': [
        { status: 200, body: [standard, { ...suite, hasRooms: true }] },
        { status: 200, body: [standard, suite] },
      ],
      'GET /rooms': [{ status: 200, body: [{ ...room101, roomCategory: 'Svit' }] }, { status: 200, body: [room101] }],
      'GET /bookings': { status: 200, body: [] },
      'PUT /rooms/101': { status: 200, body: room101 },
    });
    renderPage(<App />);
    await userEvent.click(await roomButton('101', 'Ändra'));
    const categoryOfRoom = screen.getByRole('combobox', { name: 'Kategori för rum 101' });
    await userEvent.selectOptions(categoryOfRoom, await within(categoryOfRoom).findByRole('option', { name: 'Standard' }));
    await userEvent.click(await roomButton('101', 'Spara'));
    await waitFor(async () =>
      expect(within(await rowOf('Svit')).getByRole('button', { name: 'Ta bort' })).toBeEnabled());
  });

  it('stops offering a room for booking as soon as it is deleted', async () => {
    fakeHotel({
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
    fakeHotel({
      'GET /rooms': [{ status: 200, body: [room101] }, { status: 200, body: [withBookingsToCancel] }],
      'GET /bookings': [{ status: 200, body: [] }, { status: 200, body: [booking10001] }],
      'POST /bookings': { status: 201, body: booking10001 },
    });
    renderPage(<App />);
    await waitFor(() => expect(offered('101')).toBeInTheDocument());
    await userEvent.selectOptions(roomChoice(), '101');
    await userEvent.type(screen.getByLabelText('Antal gäster'), String(booking10001.info.guestCount));
    await userEvent.type(screen.getByLabelText('Gäst'), booking10001.info.guestName);
    await userEvent.type(screen.getByLabelText('Från'), booking10001.period.from);
    await userEvent.type(screen.getByLabelText('Till'), booking10001.period.to);
    await userEvent.click(screen.getByRole('button', { name: 'Boka' }));
    await waitFor(async () => expect(await roomButton('101', 'Ta bort')).toBeDisabled());
  });

  it('lets a room be deleted as soon as its last open booking is cancelled', async () => {
    fakeHotel({
      'GET /rooms': [{ status: 200, body: [withBookingsToCancel] }, { status: 200, body: [room101] }],
      'GET /bookings': [
        { status: 200, body: [booking10001] },
        { status: 200, body: [inState('Cancelled', [])] },
      ],
      'DELETE /bookings/10001': { status: 204 },
    });
    renderPage(<App />);
    const booking = await screen.findByRole('row', { name: /^10001/ });
    await userEvent.click(within(booking).getByRole('button', { name: 'Avboka' }));
    await waitFor(async () => expect(await roomButton('101', 'Ta bort')).toBeEnabled());
  });

  it("shows a room's bookings as cancelled as soon as they are cancelled", async () => {
    fakeHotel({
      'GET /rooms': [{ status: 200, body: [withBookingsToCancel] }, { status: 200, body: [room101] }],
      'GET /bookings': [
        { status: 200, body: [booking10001] },
        { status: 200, body: [inState('Cancelled', [])] },
      ],
      'DELETE /rooms/101/bookings': { status: 204 },
    });
    renderPage(<App />);
    await userEvent.click(await roomButton('101', 'Avboka alla'));
    await answer('Avboka alla bokningar', 'OK');
    await waitFor(async () =>
      expect(await screen.findByRole('row', { name: /^10001/ })).toHaveTextContent(/AvbokadÄndra$/));
  });

  it("stops a room's bookings being cancelled as soon as its guest checks in", async () => {
    fakeHotel({
      'GET /rooms': [{ status: 200, body: [withBookingsToCancel] }, { status: 200, body: [withGuestCheckedIn] }],
      'GET /bookings': [
        { status: 200, body: [inState('Open', ['Cancel', 'CheckIn'])] },
        { status: 200, body: [inState('CheckedIn', [])] },
      ],
      'POST /bookings/10001/check-in': { status: 204 },
    });
    renderPage(<App />);
    await userEvent.click(within(await rowOf('10001')).getByRole('button', { name: 'Checka in' }));
    await waitFor(async () => expect(await roomButton('101', 'Avboka alla')).toBeDisabled());
    expect(await roomButton('101', 'Ta bort')).toBeDisabled();
  });

  it('lets a room be deleted as soon as its booking moves to another room', async () => {
    const moved = { ...booking10001, info: { ...booking10001.info, roomNumber: '102' } };
    fakeHotel({
      'GET /rooms': [
        { status: 200, body: [withBookingsToCancel, room102] },
        { status: 200, body: [room101, { ...room102, held: true, hasCancellableBookings: true }] },
      ],
      'GET /bookings': [{ status: 200, body: [booking10001] }, { status: 200, body: [moved] }],
      'PUT /bookings/10001': { status: 200, body: moved },
    });
    renderPage(<App />);
    await userEvent.click(within(await rowOf('10001')).getByRole('button', { name: 'Ändra' }));
    const roomOfBooking = screen.getByRole('combobox', { name: 'Rum i bokning 10001' });
    await userEvent.selectOptions(roomOfBooking, await within(roomOfBooking).findByRole('option', { name: '102' }));
    await userEvent.click(within(await rowOf('10001')).getByRole('button', { name: 'Spara' }));
    await waitFor(async () => expect(await roomButton('101', 'Ta bort')).toBeEnabled());
    expect(await roomButton('101', 'Avboka alla')).toBeDisabled();
  });

  it('lets a room be deleted as soon as its guest checks out', async () => {
    fakeHotel({
      'GET /rooms': [{ status: 200, body: [withGuestCheckedIn] }, { status: 200, body: [room101] }],
      'GET /bookings': [
        { status: 200, body: [inState('CheckedIn', ['CheckOut'])] },
        { status: 200, body: [inState('CheckedOut', [])] },
      ],
      'POST /bookings/10001/check-out': { status: 204 },
    });
    renderPage(<App />);
    await userEvent.click(within(await rowOf('10001')).getByRole('button', { name: 'Checka ut' }));
    await waitFor(async () => expect(await roomButton('101', 'Ta bort')).toBeEnabled());
  });
});
