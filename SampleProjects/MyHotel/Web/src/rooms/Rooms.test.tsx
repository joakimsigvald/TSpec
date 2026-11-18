import { screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';
import { answer, dialogNamed } from '../test/answer';
import { fakeApi } from '../test/fakeApi';
import { renderPage } from '../test/renderPage';
import { Rooms } from './Rooms';
import type { RoomOverview } from './roomsApi';

const room101 = { roomNumber: '101', bedCount: 2, hasOpenBookings: false };
const room102 = { roomNumber: '102', bedCount: 1, hasOpenBookings: false };
const room103 = { roomNumber: '103', bedCount: 3, hasOpenBookings: false };
const withOpenBookings = (room: RoomOverview) => ({ ...room, hasOpenBookings: true });

const rowOf = (roomNumber: string) => screen.findByRole('row', { name: new RegExp(`^${roomNumber}`) });
const bedsIn = async (roomNumber: string) => within(await rowOf(roomNumber)).getByRole('spinbutton');
const buttonIn = async (roomNumber: string, name: string) =>
  within(await rowOf(roomNumber)).getByRole('button', { name });

const addRoom = async (roomNumber: string, beds: string) => {
  await userEvent.type(screen.getByLabelText('Rumsnummer'), roomNumber);
  await userEvent.type(screen.getByLabelText('Sängar'), beds);
  await userEvent.click(screen.getByRole('button', { name: 'Lägg till rum' }));
};

describe('Rooms', () => {
  it('lists the rooms with their beds', async () => {
    fakeApi({ 'GET /rooms': { status: 200, body: [room101, room102] } });
    renderPage(<Rooms />);
    expect(await bedsIn('101')).toHaveValue(2);
    expect(await bedsIn('102')).toHaveValue(1);
  });

  it('adds the room entered', async () => {
    const api = fakeApi({
      'GET /rooms': [{ status: 200, body: [room101] }, { status: 200, body: [room101, room103] }],
      'POST /rooms': { status: 201, body: room103 },
    });
    renderPage(<Rooms />);
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
    renderPage(<Rooms />);
    await addRoom('101', '2');
    await waitFor(() =>
      expect(screen.getByLabelText('Rumsnummer')).toHaveAccessibleDescription('Rummet finns redan.'));
  });

  it('says under the button why a room could not be added for any other reason', async () => {
    fakeApi({
      'GET /rooms': { status: 200, body: [room101] },
      'POST /rooms': { status: 500 },
    });
    renderPage(<Rooms />);
    await addRoom('103', '3');
    await waitFor(() =>
      expect(screen.getByRole('button', { name: 'Lägg till rum' })).toHaveAccessibleDescription('Något gick fel.'));
  });

  it('says something went wrong when the API gives no reason', async () => {
    fakeApi({ 'GET /rooms': { status: 500 } });
    renderPage(<Rooms />);
    expect(await screen.findByRole('alert')).toHaveTextContent('Något gick fel.');
  });

  it('saves the beds entered for a room', async () => {
    const api = fakeApi({
      'GET /rooms': [{ status: 200, body: [room101] }, { status: 200, body: [{ ...room101, bedCount: 3 }] }],
      'PUT /rooms/101': { status: 200, body: { ...room101, bedCount: 3 } },
    });
    renderPage(<Rooms />);
    const beds = await bedsIn('101');
    await userEvent.clear(beds);
    await userEvent.type(beds, '3');
    await userEvent.click(await buttonIn('101', 'Spara'));
    expect(api).toHaveBeenCalledWith('/rooms/101', expect.objectContaining({
      method: 'PUT',
      body: JSON.stringify({ roomNumber: '101', bedCount: 3 }),
    }));
  });

  it('asks before deleting a room', async () => {
    fakeApi({ 'GET /rooms': { status: 200, body: [room101] } });
    renderPage(<Rooms />);
    await userEvent.click(await buttonIn('101', 'Ta bort'));
    expect(await dialogNamed('Ta bort rum')).toHaveAccessibleDescription('Ta bort rum 101?');
  });

  it('removes a deleted room from the list, once confirmed', async () => {
    fakeApi({
      'GET /rooms': [{ status: 200, body: [room101, room102] }, { status: 200, body: [room102] }],
      'DELETE /rooms/101': { status: 204 },
    });
    renderPage(<Rooms />);
    await userEvent.click(await buttonIn('101', 'Ta bort'));
    await answer('Ta bort rum', 'OK');
    await waitFor(() => expect(screen.queryByRole('row', { name: /^101/ })).not.toBeInTheDocument());
    expect(await rowOf('102')).toBeInTheDocument();
  });

  it('keeps a room that was not confirmed deleted', async () => {
    const api = fakeApi({ 'GET /rooms': { status: 200, body: [room101] } });
    renderPage(<Rooms />);
    await userEvent.click(await buttonIn('101', 'Ta bort'));
    await answer('Ta bort rum', 'Avbryt');
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    expect(api).not.toHaveBeenCalledWith('/rooms/101', expect.objectContaining({ method: 'DELETE' }));
  });

  it('cannot delete a room with open bookings, and says why', async () => {
    fakeApi({ 'GET /rooms': { status: 200, body: [withOpenBookings(room101)] } });
    renderPage(<Rooms />);
    const remove = await buttonIn('101', 'Ta bort');
    expect(remove).toBeDisabled();
    expect(remove).toHaveAccessibleDescription('Rummet har aktuella eller kommande bokningar.');
  });

  it('says above the rooms that a room with open bookings was not deleted', async () => {
    fakeApi({
      'GET /rooms': { status: 200, body: [room101] },
      'DELETE /rooms/101': { status: 409, body: { code: 'RoomHasOpenBookings' } },
    });
    renderPage(<Rooms />);
    await userEvent.click(await buttonIn('101', 'Ta bort'));
    await answer('Ta bort rum', 'OK');
    expect(await screen.findByRole('alert')).toHaveTextContent('Rummet har aktuella eller kommande bokningar.');
  });

  it("asks before cancelling a room's open bookings", async () => {
    fakeApi({ 'GET /rooms': { status: 200, body: [withOpenBookings(room101)] } });
    renderPage(<Rooms />);
    await userEvent.click(await buttonIn('101', 'Avboka alla'));
    expect(await dialogNamed('Avboka alla bokningar'))
      .toHaveAccessibleDescription('Avboka alla aktuella och kommande bokningar för rum 101?');
  });

  it("cancels a room's open bookings, once confirmed", async () => {
    const api = fakeApi({
      'GET /rooms': [{ status: 200, body: [withOpenBookings(room101)] }, { status: 200, body: [room101] }],
      'DELETE /rooms/101/bookings': { status: 204 },
    });
    renderPage(<Rooms />);
    await userEvent.click(await buttonIn('101', 'Avboka alla'));
    await answer('Avboka alla bokningar', 'OK');
    await waitFor(async () => expect(await buttonIn('101', 'Avboka alla')).toBeDisabled());
    expect(api).toHaveBeenCalledWith('/rooms/101/bookings', expect.objectContaining({ method: 'DELETE' }));
  });

  it("keeps a room's open bookings when cancelling them was not confirmed", async () => {
    const api = fakeApi({ 'GET /rooms': { status: 200, body: [withOpenBookings(room101)] } });
    renderPage(<Rooms />);
    await userEvent.click(await buttonIn('101', 'Avboka alla'));
    await answer('Avboka alla bokningar', 'Avbryt');
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    expect(api).not.toHaveBeenCalledWith('/rooms/101/bookings', expect.anything());
  });

  it('cannot cancel the bookings of a room without open bookings, and says why', async () => {
    fakeApi({ 'GET /rooms': { status: 200, body: [room101] } });
    renderPage(<Rooms />);
    const cancelAll = await buttonIn('101', 'Avboka alla');
    expect(cancelAll).toBeDisabled();
    expect(cancelAll).toHaveAccessibleDescription('Rummet har inga aktuella eller kommande bokningar.');
  });
});
