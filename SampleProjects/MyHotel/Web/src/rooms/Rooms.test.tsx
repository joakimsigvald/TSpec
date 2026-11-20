import { screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';
import { answer, dialogNamed } from '../test/answer';
import { fakeApi } from '../test/fakeApi';
import { renderPage } from '../test/renderPage';
import { Rooms } from './Rooms';
import type { RoomOverview } from './roomsApi';

const unbooked = { held: false, hasCancellableBookings: false };
const room101 = { roomNumber: '101', bedCount: 2, roomCategory: 'Standard', ...unbooked };
const room102 = { roomNumber: '102', bedCount: 1, roomCategory: 'Standard', ...unbooked };
const room103 = { roomNumber: '103', bedCount: 3, roomCategory: 'Standard', ...unbooked };
const withBookingsToCancel = (room: RoomOverview) => ({ ...room, held: true, hasCancellableBookings: true });
const withGuestCheckedIn = (room: RoomOverview) => ({ ...room, held: true });

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
const buttonsIn = async (roomNumber: string) =>
  within(await rowOf(roomNumber)).getAllByRole('button').map(button => button.textContent);
const startChanging = async (roomNumber: string) => userEvent.click(await buttonIn(roomNumber, 'Ändra'));

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
  it('lists the rooms with their categories and beds', async () => {
    fakeHotel({ 'GET /rooms': { status: 200, body: [room101, { ...room102, roomCategory: 'Svit' }] } });
    renderPage(<Rooms />);
    expect(await rowOf('101')).toHaveTextContent(/^101Standard2Avboka/);
    expect(await rowOf('102')).toHaveTextContent(/^102Svit1Avboka/);
  });

  it('offers to change a room, last in its row', async () => {
    fakeHotel({ 'GET /rooms': { status: 200, body: [room101] } });
    renderPage(<Rooms />);
    expect(await buttonsIn('101')).toEqual(['Avboka alla', 'Ta bort', 'Ändra']);
  });

  it("turns the row into fields holding the room's values", async () => {
    fakeHotel({ 'GET /rooms': { status: 200, body: [room101] } });
    renderPage(<Rooms />);
    await startChanging('101');
    await waitFor(async () => expect(await categoryIn('101')).toHaveDisplayValue('Standard'));
    await waitFor(async () => expect(await bedsIn('101')).toHaveDisplayValue('2'));
    expect(await buttonsIn('101')).toEqual(['Spara', 'Avbryt']);
  });

  it('offers the numbers of beds the rules allow, in the form and for each room', async () => {
    fakeHotel({
      'GET /rooms': { status: 200, body: [room101] },
      'GET /rules': { status: 200, body: { minBedCount: 2, maxBedCount: 4 } },
    });
    renderPage(<Rooms />);
    await waitFor(() => expect(optionsIn(bedsChoice())).toEqual(['2', '3', '4']));
    await startChanging('101');
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
    expect(await rowOf('103')).toBeInTheDocument();
    expect(api).toHaveBeenCalledWith('/rooms', expect.objectContaining({
      method: 'POST',
      body: JSON.stringify({ roomNumber: '103', bedCount: 3, roomCategory: 'Svit' }),
    }));
    expect(categoryChoice()).toHaveDisplayValue('Välj kategori');
    expect(bedsChoice()).toHaveDisplayValue('1');
  });

  it('adds a room with the fewest beds when no number of beds is chosen', async () => {
    const api = fakeHotel({
      'GET /rooms': [{ status: 200, body: [] }, { status: 200, body: [room102] }],
      'POST /rooms': { status: 201, body: room102 },
    });
    renderPage(<Rooms />);
    await waitFor(() => expect(bedsChoice()).toHaveDisplayValue('1'));
    await userEvent.type(screen.getByLabelText('Rumsnummer'), '102');
    await choose(categoryChoice(), 'Standard');
    await userEvent.click(addButton());
    await rowOf('102');
    expect(api).toHaveBeenCalledWith('/rooms', expect.objectContaining({
      method: 'POST',
      body: JSON.stringify({ roomNumber: '102', bedCount: 1, roomCategory: 'Standard' }),
    }));
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
    await startChanging('101');
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
    await startChanging('101');
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
    await startChanging('101');
    expect(await buttonIn('101', 'Spara')).toBeDisabled();
  });

  it('cannot save a room whose value is changed back', async () => {
    fakeHotel({ 'GET /rooms': { status: 200, body: [room101] } });
    renderPage(<Rooms />);
    await startChanging('101');
    await choose(await bedsIn('101'), '3');
    await choose(await bedsIn('101'), '2');
    expect(await buttonIn('101', 'Spara')).toBeDisabled();
  });

  it('shows a saved change on the row', async () => {
    fakeHotel({
      'GET /rooms': [{ status: 200, body: [room101] }, { status: 200, body: [{ ...room101, bedCount: 3 }] }],
      'PUT /rooms/101': { status: 200, body: { ...room101, bedCount: 3 } },
    });
    renderPage(<Rooms />);
    await startChanging('101');
    await choose(await bedsIn('101'), '3');
    await userEvent.click(await buttonIn('101', 'Spara'));
    await waitFor(async () => expect(await buttonsIn('101')).toEqual(['Avboka alla', 'Ta bort', 'Ändra']));
    expect(await rowOf('101')).toHaveTextContent(/^101Standard3Avboka/);
  });

  it('shows the row as it was when the change is abandoned', async () => {
    fakeHotel({ 'GET /rooms': { status: 200, body: [room101] } });
    renderPage(<Rooms />);
    await startChanging('101');
    await choose(await bedsIn('101'), '3');
    await userEvent.click(await buttonIn('101', 'Avbryt'));
    expect(await rowOf('101')).toHaveTextContent(/^101Standard2Avboka/);
    expect(await buttonsIn('101')).toEqual(['Avboka alla', 'Ta bort', 'Ändra']);
    await startChanging('101');
    expect(await bedsIn('101')).toHaveDisplayValue('2');
  });

  describe('says on the row why a change was refused', () => {
    const changeRefusedWith = async (status: number, code: string) => {
      fakeHotel({
        'GET /rooms': { status: 200, body: [room101] },
        'PUT /rooms/101': { status, body: { code } },
      });
      renderPage(<Rooms />);
      await startChanging('101');
      await choose(await bedsIn('101'), '3');
      await userEvent.click(await buttonIn('101', 'Spara'));
    };

    it('under the beds, that the number of beds is not allowed', async () => {
      await changeRefusedWith(400, 'InvalidBedCount');
      await waitFor(async () => expect(await bedsIn('101')).toHaveAccessibleDescription('Otillåtet antal sängar.'));
    });

    it('under the category, that it does not exist', async () => {
      await changeRefusedWith(404, 'RoomCategoryNotFound');
      await waitFor(async () => expect(await categoryIn('101')).toHaveAccessibleDescription('Kategorin finns inte.'));
    });

    it('under the button, for any other reason', async () => {
      await changeRefusedWith(404, 'RoomNotFound');
      await waitFor(async () =>
        expect(await buttonIn('101', 'Spara')).toHaveAccessibleDescription('Rummet finns inte.'));
    });
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

  it('cannot delete a room a booking holds, and says why', async () => {
    fakeHotel({ 'GET /rooms': { status: 200, body: [withBookingsToCancel(room101)] } });
    renderPage(<Rooms />);
    const remove = await buttonIn('101', 'Ta bort');
    expect(remove).toBeDisabled();
    expect(remove).toHaveAccessibleDescription('Rummet har aktuella eller kommande bokningar.');
  });

  it('cannot delete a room whose guest is checked in, nor cancel its bookings', async () => {
    fakeHotel({ 'GET /rooms': { status: 200, body: [withGuestCheckedIn(room101)] } });
    renderPage(<Rooms />);
    expect(await buttonIn('101', 'Ta bort')).toBeDisabled();
    expect(await buttonIn('101', 'Avboka alla')).toBeDisabled();
  });

  it('says above the rooms that a room a booking holds was not deleted', async () => {
    fakeHotel({
      'GET /rooms': { status: 200, body: [room101] },
      'DELETE /rooms/101': { status: 409, body: { code: 'RoomHeld' } },
    });
    renderPage(<Rooms />);
    await userEvent.click(await buttonIn('101', 'Ta bort'));
    await answer('Ta bort rum', 'OK');
    expect(await screen.findByRole('alert')).toHaveTextContent('Rummet har aktuella eller kommande bokningar.');
  });

  it("asks before cancelling a room's bookings", async () => {
    fakeHotel({ 'GET /rooms': { status: 200, body: [withBookingsToCancel(room101)] } });
    renderPage(<Rooms />);
    await userEvent.click(await buttonIn('101', 'Avboka alla'));
    expect(await dialogNamed('Avboka alla bokningar'))
      .toHaveAccessibleDescription('Avboka alla bokningar som går att avboka för rum 101?');
  });

  it("cancels a room's bookings, once confirmed", async () => {
    const api = fakeHotel({
      'GET /rooms': [{ status: 200, body: [withBookingsToCancel(room101)] }, { status: 200, body: [room101] }],
      'DELETE /rooms/101/bookings': { status: 204 },
    });
    renderPage(<Rooms />);
    await userEvent.click(await buttonIn('101', 'Avboka alla'));
    await answer('Avboka alla bokningar', 'OK');
    await waitFor(async () => expect(await buttonIn('101', 'Avboka alla')).toBeDisabled());
    expect(api).toHaveBeenCalledWith('/rooms/101/bookings', expect.objectContaining({ method: 'DELETE' }));
  });

  it("keeps a room's bookings when cancelling them was not confirmed", async () => {
    const api = fakeHotel({ 'GET /rooms': { status: 200, body: [withBookingsToCancel(room101)] } });
    renderPage(<Rooms />);
    await userEvent.click(await buttonIn('101', 'Avboka alla'));
    await answer('Avboka alla bokningar', 'Avbryt');
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    expect(api).not.toHaveBeenCalledWith('/rooms/101/bookings', expect.anything());
  });

  it('cannot cancel the bookings of a room without bookings to cancel, and says why', async () => {
    fakeHotel({ 'GET /rooms': { status: 200, body: [room101] } });
    renderPage(<Rooms />);
    const cancelAll = await buttonIn('101', 'Avboka alla');
    expect(cancelAll).toBeDisabled();
    expect(cancelAll).toHaveAccessibleDescription('Rummet har inga bokningar att avboka.');
  });
});
