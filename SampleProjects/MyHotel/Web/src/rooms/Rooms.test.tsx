import { screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';
import { answer, dialogNamed } from '../test/answer';
import { fakeApi } from '../test/fakeApi';
import { renderPage } from '../test/renderPage';
import { Rooms } from './Rooms';
import type { RoomOverview } from './roomsApi';

const room101 = { roomNumber: '101', bedCount: 2, roomCategory: 'Standard', hasOpenBookings: false };
const room102 = { roomNumber: '102', bedCount: 1, roomCategory: 'Standard', hasOpenBookings: false };
const room103 = { roomNumber: '103', bedCount: 3, roomCategory: 'Standard', hasOpenBookings: false };
const withOpenBookings = (room: RoomOverview) => ({ ...room, hasOpenBookings: true });

const roomCategories = ['Örnrum', 'Ängsrum', 'Svit', 'Åsrum', 'Standard']
  .map(name => ({ name, description: name, hasRooms: false }));

const fakeHotel = (replies: Parameters<typeof fakeApi>[0]) =>
  fakeApi({
    'GET /room-categories': { status: 200, body: roomCategories },
    'GET /rules': { status: 200, body: { minBedCount: 1, maxBedCount: 9 } },
    ...replies,
  });

const rowOf = (roomNumber: string) => screen.findByRole('row', { name: new RegExp(`^${roomNumber}`) });
const bedsIn = async (roomNumber: string) =>
  within(await rowOf(roomNumber)).getByRole('combobox', { name: `Sängar i rum ${roomNumber}` });
const categoryIn = async (roomNumber: string) =>
  within(await rowOf(roomNumber)).getByRole('combobox', { name: `Kategori för rum ${roomNumber}` });
const buttonIn = async (roomNumber: string, name: string) =>
  within(await rowOf(roomNumber)).getByRole('button', { name });

const categoryChoice = () => screen.getByRole('combobox', { name: 'Kategori' });
const bedsChoice = () => screen.getByRole('combobox', { name: 'Sängar' });
const optionsIn = (choice: HTMLElement) => within(choice).getAllByRole('option').map(option => option.textContent);
const addButton = () => screen.getByRole('button', { name: 'Lägg till rum' });

const choose = async (choice: HTMLElement, name: string) =>
  userEvent.selectOptions(choice, await within(choice).findByRole('option', { name }));

const addRoom = async (roomNumber: string, beds: string, roomCategory = 'Standard') => {
  await userEvent.type(screen.getByLabelText('Rumsnummer'), roomNumber);
  await choose(bedsChoice(), beds);
  await choose(categoryChoice(), roomCategory);
  await userEvent.click(addButton());
};

describe('Rooms', () => {
  it('lists the rooms with their beds and categories', async () => {
    fakeHotel({ 'GET /rooms': { status: 200, body: [room101, { ...room102, roomCategory: 'Svit' }] } });
    renderPage(<Rooms />);
    await waitFor(async () => expect(await bedsIn('101')).toHaveDisplayValue('2'));
    expect(await bedsIn('102')).toHaveDisplayValue('1');
    await waitFor(async () => expect(await categoryIn('101')).toHaveDisplayValue('Standard'));
    expect(await categoryIn('102')).toHaveDisplayValue('Svit');
  });

  it('offers the numbers of beds the rules allow, in the form and for each room', async () => {
    fakeHotel({
      'GET /rooms': { status: 200, body: [room101] },
      'GET /rules': { status: 200, body: { minBedCount: 2, maxBedCount: 4 } },
    });
    renderPage(<Rooms />);
    await waitFor(() => expect(optionsIn(bedsChoice())).toEqual(['2', '3', '4']));
    expect(optionsIn(await bedsIn('101'))).toEqual(['2', '3', '4']);
  });

  it('offers the fewest beds in the form until another number is chosen', async () => {
    fakeHotel({ 'GET /rooms': { status: 200, body: [] } });
    renderPage(<Rooms />);
    await waitFor(() => expect(bedsChoice()).toHaveDisplayValue('1'));
  });

  it('offers the categories by name the Swedish way', async () => {
    fakeHotel({ 'GET /rooms': { status: 200, body: [] } });
    renderPage(<Rooms />);
    await waitFor(() => expect(within(categoryChoice()).getAllByRole('option').map(option => option.textContent))
      .toEqual(['Välj kategori', 'Standard', 'Svit', 'Åsrum', 'Ängsrum', 'Örnrum']));
  });

  it('adds the room entered, and empties the form', async () => {
    const api = fakeHotel({
      'GET /rooms': [{ status: 200, body: [room101] }, { status: 200, body: [room101, room103] }],
      'POST /rooms': { status: 201, body: room103 },
    });
    renderPage(<Rooms />);
    await addRoom('103', '3', 'Svit');
    expect(await bedsIn('103')).toHaveDisplayValue('3');
    expect(api).toHaveBeenCalledWith('/rooms', expect.objectContaining({
      method: 'POST',
      body: JSON.stringify({ roomNumber: '103', bedCount: 3, roomCategory: 'Svit' }),
    }));
    expect(categoryChoice()).toHaveDisplayValue('Välj kategori');
    expect(bedsChoice()).toHaveDisplayValue('1');
  });

  it('cannot add a room without a room number', async () => {
    fakeHotel({ 'GET /rooms': { status: 200, body: [] } });
    renderPage(<Rooms />);
    await choose(categoryChoice(), 'Standard');
    expect(addButton()).toBeDisabled();
  });

  it('cannot add a room without a category', async () => {
    fakeHotel({ 'GET /rooms': { status: 200, body: [] } });
    renderPage(<Rooms />);
    await userEvent.type(screen.getByLabelText('Rumsnummer'), '103');
    expect(addButton()).toBeDisabled();
  });

  it('cannot add a room again until another is entered', async () => {
    fakeHotel({
      'GET /rooms': [{ status: 200, body: [room101] }, { status: 200, body: [room101, room103] }],
      'POST /rooms': { status: 201, body: room103 },
    });
    renderPage(<Rooms />);
    await addRoom('103', '3');
    await waitFor(() => expect(addButton()).toBeDisabled());
  });

  it('says under the beds that the number of beds is not allowed', async () => {
    fakeHotel({
      'GET /rooms': { status: 200, body: [room101] },
      'POST /rooms': { status: 400, body: { code: 'InvalidBedCount' } },
    });
    renderPage(<Rooms />);
    await addRoom('103', '3');
    await waitFor(() => expect(bedsChoice()).toHaveAccessibleDescription('Otillåtet antal sängar.'));
  });

  it('says under the category that it does not exist', async () => {
    fakeHotel({
      'GET /rooms': { status: 200, body: [room101] },
      'POST /rooms': { status: 404, body: { code: 'RoomCategoryNotFound' } },
    });
    renderPage(<Rooms />);
    await addRoom('103', '3');
    await waitFor(() => expect(categoryChoice()).toHaveAccessibleDescription('Kategorin finns inte.'));
  });

  it('says under the room number that the room already exists', async () => {
    fakeHotel({
      'GET /rooms': { status: 200, body: [room101] },
      'POST /rooms': { status: 409, body: { code: 'RoomAlreadyExists' } },
    });
    renderPage(<Rooms />);
    await addRoom('101', '2');
    await waitFor(() =>
      expect(screen.getByLabelText('Rumsnummer')).toHaveAccessibleDescription('Rummet finns redan.'));
  });

  it('says under the button why a room could not be added for any other reason', async () => {
    fakeHotel({
      'GET /rooms': { status: 200, body: [room101] },
      'POST /rooms': { status: 500 },
    });
    renderPage(<Rooms />);
    await addRoom('103', '3');
    await waitFor(() => expect(addButton()).toHaveAccessibleDescription('Något gick fel.'));
  });

  it('says something went wrong when the API gives no reason', async () => {
    fakeHotel({ 'GET /rooms': { status: 500 } });
    renderPage(<Rooms />);
    expect(await screen.findByRole('alert')).toHaveTextContent('Något gick fel.');
  });

  it('saves the beds chosen for a room', async () => {
    const api = fakeHotel({
      'GET /rooms': [{ status: 200, body: [room101] }, { status: 200, body: [{ ...room101, bedCount: 3 }] }],
      'PUT /rooms/101': { status: 200, body: { ...room101, bedCount: 3 } },
    });
    renderPage(<Rooms />);
    await choose(await bedsIn('101'), '3');
    await userEvent.click(await buttonIn('101', 'Spara'));
    expect(api).toHaveBeenCalledWith('/rooms/101', expect.objectContaining({
      method: 'PUT',
      body: JSON.stringify({ roomNumber: '101', bedCount: 3, roomCategory: 'Standard' }),
    }));
  });

  it('moves a room to the category chosen', async () => {
    const api = fakeHotel({
      'GET /rooms': [{ status: 200, body: [room101] }, { status: 200, body: [{ ...room101, roomCategory: 'Svit' }] }],
      'PUT /rooms/101': { status: 200, body: { ...room101, roomCategory: 'Svit' } },
    });
    renderPage(<Rooms />);
    await choose(await categoryIn('101'), 'Svit');
    await userEvent.click(await buttonIn('101', 'Spara'));
    expect(api).toHaveBeenCalledWith('/rooms/101', expect.objectContaining({
      method: 'PUT',
      body: JSON.stringify({ roomNumber: '101', bedCount: 2, roomCategory: 'Svit' }),
    }));
  });

  it('cannot save a room before a value on its row changes', async () => {
    fakeHotel({ 'GET /rooms': { status: 200, body: [room101] } });
    renderPage(<Rooms />);
    expect(await buttonIn('101', 'Spara')).toBeDisabled();
  });

  it('cannot save a room whose value is changed back', async () => {
    fakeHotel({ 'GET /rooms': { status: 200, body: [room101] } });
    renderPage(<Rooms />);
    await choose(await bedsIn('101'), '3');
    await choose(await bedsIn('101'), '2');
    expect(await buttonIn('101', 'Spara')).toBeDisabled();
  });

  it('cannot save a room again once it is saved', async () => {
    fakeHotel({
      'GET /rooms': [{ status: 200, body: [room101] }, { status: 200, body: [{ ...room101, bedCount: 3 }] }],
      'PUT /rooms/101': { status: 200, body: { ...room101, bedCount: 3 } },
    });
    renderPage(<Rooms />);
    await choose(await bedsIn('101'), '3');
    await userEvent.click(await buttonIn('101', 'Spara'));
    await waitFor(async () => expect(await buttonIn('101', 'Spara')).toBeDisabled());
  });

  it('asks before deleting a room', async () => {
    fakeHotel({ 'GET /rooms': { status: 200, body: [room101] } });
    renderPage(<Rooms />);
    await userEvent.click(await buttonIn('101', 'Ta bort'));
    expect(await dialogNamed('Ta bort rum')).toHaveAccessibleDescription('Ta bort rum 101?');
  });

  it('removes a deleted room from the list, once confirmed', async () => {
    fakeHotel({
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
    const api = fakeHotel({ 'GET /rooms': { status: 200, body: [room101] } });
    renderPage(<Rooms />);
    await userEvent.click(await buttonIn('101', 'Ta bort'));
    await answer('Ta bort rum', 'Avbryt');
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    expect(api).not.toHaveBeenCalledWith('/rooms/101', expect.objectContaining({ method: 'DELETE' }));
  });

  it('cannot delete a room with open bookings, and says why', async () => {
    fakeHotel({ 'GET /rooms': { status: 200, body: [withOpenBookings(room101)] } });
    renderPage(<Rooms />);
    const remove = await buttonIn('101', 'Ta bort');
    expect(remove).toBeDisabled();
    expect(remove).toHaveAccessibleDescription('Rummet har aktuella eller kommande bokningar.');
  });

  it('says above the rooms that a room with open bookings was not deleted', async () => {
    fakeHotel({
      'GET /rooms': { status: 200, body: [room101] },
      'DELETE /rooms/101': { status: 409, body: { code: 'RoomHasOpenBookings' } },
    });
    renderPage(<Rooms />);
    await userEvent.click(await buttonIn('101', 'Ta bort'));
    await answer('Ta bort rum', 'OK');
    expect(await screen.findByRole('alert')).toHaveTextContent('Rummet har aktuella eller kommande bokningar.');
  });

  it("asks before cancelling a room's open bookings", async () => {
    fakeHotel({ 'GET /rooms': { status: 200, body: [withOpenBookings(room101)] } });
    renderPage(<Rooms />);
    await userEvent.click(await buttonIn('101', 'Avboka alla'));
    expect(await dialogNamed('Avboka alla bokningar'))
      .toHaveAccessibleDescription('Avboka alla aktuella och kommande bokningar för rum 101?');
  });

  it("cancels a room's open bookings, once confirmed", async () => {
    const api = fakeHotel({
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
    const api = fakeHotel({ 'GET /rooms': { status: 200, body: [withOpenBookings(room101)] } });
    renderPage(<Rooms />);
    await userEvent.click(await buttonIn('101', 'Avboka alla'));
    await answer('Avboka alla bokningar', 'Avbryt');
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    expect(api).not.toHaveBeenCalledWith('/rooms/101/bookings', expect.anything());
  });

  it('cannot cancel the bookings of a room without open bookings, and says why', async () => {
    fakeHotel({ 'GET /rooms': { status: 200, body: [room101] } });
    renderPage(<Rooms />);
    const cancelAll = await buttonIn('101', 'Avboka alla');
    expect(cancelAll).toBeDisabled();
    expect(cancelAll).toHaveAccessibleDescription('Rummet har inga aktuella eller kommande bokningar.');
  });
});
