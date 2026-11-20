import { screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';
import { answer, dialogNamed } from '../test/answer';
import { fakeApi } from '../test/fakeApi';
import { renderPage } from '../test/renderPage';
import { RoomCategories } from './RoomCategories';

const standard = { name: 'Standard', description: 'Standardrum', hasRooms: false };
const suite = { name: 'Svit', description: 'Två rum', hasRooms: false };

const rowOf = (name: string) => screen.findByRole('row', { name: new RegExp(`^${name}`) });
const descriptionIn = async (name: string) => within(await rowOf(name)).getByRole('textbox');
const buttonIn = async (name: string, button: string) =>
  within(await rowOf(name)).getByRole('button', { name: button });
const buttonsIn = async (name: string) =>
  within(await rowOf(name)).getAllByRole('button').map(button => button.textContent);
const startChanging = async (name: string) => userEvent.click(await buttonIn(name, 'Ändra'));

const listed = () => screen.getAllByRole('row').slice(1).map(row => within(row).getAllByRole('cell')[0].textContent);
const addButton = () => screen.getByRole('button', { name: 'Lägg till kategori' });

const addRoomCategory = async (name: string, description: string) => {
  await userEvent.type(screen.getByLabelText('Namn'), name);
  await userEvent.type(screen.getByLabelText('Beskrivning'), description);
  await userEvent.click(addButton());
};

describe('RoomCategories', () => {
  it('lists the categories with their descriptions', async () => {
    fakeApi({ 'GET /room-categories': { status: 200, body: [standard, suite] } });
    renderPage(<RoomCategories />);
    expect(await rowOf('Standard')).toHaveTextContent(/^StandardStandardrumTa bort/);
    expect(await rowOf('Svit')).toHaveTextContent(/^SvitTvå rumTa bort/);
  });

  it('offers to change a category, last in its row', async () => {
    fakeApi({ 'GET /room-categories': { status: 200, body: [suite] } });
    renderPage(<RoomCategories />);
    expect(await buttonsIn('Svit')).toEqual(['Ta bort', 'Ändra']);
  });

  it('turns the description into a field holding it', async () => {
    fakeApi({ 'GET /room-categories': { status: 200, body: [suite] } });
    renderPage(<RoomCategories />);
    await startChanging('Svit');
    expect(await descriptionIn('Svit')).toHaveValue('Två rum');
    expect(await buttonsIn('Svit')).toEqual(['Spara', 'Avbryt']);
  });

  it('lists the categories by name the Swedish way', async () => {
    const named = (name: string) => ({ ...standard, name });
    fakeApi({
      'GET /room-categories': { status: 200, body: ['Örnrum', 'Ängsrum', 'Svit', 'Åsrum', 'Standard'].map(named) },
    });
    renderPage(<RoomCategories />);
    await waitFor(() => expect(listed()).toEqual(['Standard', 'Svit', 'Åsrum', 'Ängsrum', 'Örnrum']));
  });

  it('adds the category entered', async () => {
    const api = fakeApi({
      'GET /room-categories': [{ status: 200, body: [standard] }, { status: 200, body: [standard, suite] }],
      'POST /room-categories': { status: 201, body: suite },
    });
    renderPage(<RoomCategories />);
    await addRoomCategory('Svit', 'Två rum');
    expect(await rowOf('Svit')).toHaveTextContent(/^SvitTvå rumTa bort/);
    expect(api).toHaveBeenCalledWith('/room-categories', expect.objectContaining({
      method: 'POST',
      body: JSON.stringify({ name: 'Svit', description: 'Två rum' }),
    }));
  });

  it('cannot add a category without a name', async () => {
    fakeApi({ 'GET /room-categories': { status: 200, body: [standard] } });
    renderPage(<RoomCategories />);
    await userEvent.type(screen.getByLabelText('Beskrivning'), 'Två rum');
    expect(addButton()).toBeDisabled();
  });

  it('cannot add a category without a description', async () => {
    fakeApi({ 'GET /room-categories': { status: 200, body: [standard] } });
    renderPage(<RoomCategories />);
    await userEvent.type(screen.getByLabelText('Namn'), 'Svit');
    expect(addButton()).toBeDisabled();
  });

  it('cannot add a category again until another is entered', async () => {
    fakeApi({
      'GET /room-categories': [{ status: 200, body: [standard] }, { status: 200, body: [standard, suite] }],
      'POST /room-categories': { status: 201, body: suite },
    });
    renderPage(<RoomCategories />);
    await addRoomCategory('Svit', 'Två rum');
    await waitFor(() => expect(addButton()).toBeDisabled());
  });

  it('says under the name that the category already exists', async () => {
    fakeApi({
      'GET /room-categories': { status: 200, body: [standard] },
      'POST /room-categories': { status: 409, body: { code: 'RoomCategoryAlreadyExists' } },
    });
    renderPage(<RoomCategories />);
    await addRoomCategory('Standard', 'Vanligt rum');
    await waitFor(() =>
      expect(screen.getByLabelText('Namn')).toHaveAccessibleDescription('Kategorin finns redan.'));
  });

  it('saves the description entered for a category', async () => {
    const api = fakeApi({
      'GET /room-categories': [
        { status: 200, body: [suite] },
        { status: 200, body: [{ ...suite, description: 'Stor' }] },
      ],
      'PUT /room-categories/Svit': { status: 200, body: { ...suite, description: 'Stor' } },
    });
    renderPage(<RoomCategories />);
    await startChanging('Svit');
    const description = await descriptionIn('Svit');
    await userEvent.clear(description);
    await userEvent.type(description, 'Stor');
    await userEvent.click(await buttonIn('Svit', 'Spara'));
    expect(api).toHaveBeenCalledWith('/room-categories/Svit', expect.objectContaining({
      method: 'PUT',
      body: JSON.stringify({ name: 'Svit', description: 'Stor' }),
    }));
  });

  it('cannot save a category before its description changes', async () => {
    fakeApi({ 'GET /room-categories': { status: 200, body: [suite] } });
    renderPage(<RoomCategories />);
    await startChanging('Svit');
    expect(await buttonIn('Svit', 'Spara')).toBeDisabled();
  });

  it('cannot save a category whose description is changed back', async () => {
    fakeApi({ 'GET /room-categories': { status: 200, body: [suite] } });
    renderPage(<RoomCategories />);
    await startChanging('Svit');
    const description = await descriptionIn('Svit');
    await userEvent.type(description, '!');
    await userEvent.type(description, '{Backspace}');
    expect(await buttonIn('Svit', 'Spara')).toBeDisabled();
  });

  it('cannot save a category without a description', async () => {
    fakeApi({ 'GET /room-categories': { status: 200, body: [suite] } });
    renderPage(<RoomCategories />);
    await startChanging('Svit');
    await userEvent.clear(await descriptionIn('Svit'));
    expect(await buttonIn('Svit', 'Spara')).toBeDisabled();
  });

  it('shows a saved change on the row', async () => {
    fakeApi({
      'GET /room-categories': [
        { status: 200, body: [suite] },
        { status: 200, body: [{ ...suite, description: 'Stor' }] },
      ],
      'PUT /room-categories/Svit': { status: 200, body: { ...suite, description: 'Stor' } },
    });
    renderPage(<RoomCategories />);
    await startChanging('Svit');
    const description = await descriptionIn('Svit');
    await userEvent.clear(description);
    await userEvent.type(description, 'Stor');
    await userEvent.click(await buttonIn('Svit', 'Spara'));
    await waitFor(async () => expect(await buttonsIn('Svit')).toEqual(['Ta bort', 'Ändra']));
    expect(await rowOf('Svit')).toHaveTextContent(/^SvitStorTa bort/);
  });

  it('shows the row as it was when the change is abandoned', async () => {
    fakeApi({ 'GET /room-categories': { status: 200, body: [suite] } });
    renderPage(<RoomCategories />);
    await startChanging('Svit');
    await userEvent.type(await descriptionIn('Svit'), '!');
    await userEvent.click(await buttonIn('Svit', 'Avbryt'));
    expect(await rowOf('Svit')).toHaveTextContent(/^SvitTvå rumTa bort/);
    expect(await buttonsIn('Svit')).toEqual(['Ta bort', 'Ändra']);
    await startChanging('Svit');
    expect(await descriptionIn('Svit')).toHaveValue('Två rum');
  });

  it('says under the button on the row why a change was refused', async () => {
    fakeApi({
      'GET /room-categories': { status: 200, body: [suite] },
      'PUT /room-categories/Svit': { status: 404, body: { code: 'RoomCategoryNotFound' } },
    });
    renderPage(<RoomCategories />);
    await startChanging('Svit');
    await userEvent.type(await descriptionIn('Svit'), '!');
    await userEvent.click(await buttonIn('Svit', 'Spara'));
    await waitFor(async () =>
      expect(await buttonIn('Svit', 'Spara')).toHaveAccessibleDescription('Kategorin finns inte.'));
  });

  it('asks before deleting a category', async () => {
    fakeApi({ 'GET /room-categories': { status: 200, body: [standard, suite] } });
    renderPage(<RoomCategories />);
    await userEvent.click(await buttonIn('Svit', 'Ta bort'));
    expect(await dialogNamed('Ta bort kategori')).toHaveAccessibleDescription('Ta bort kategori Svit?');
  });

  it('removes a deleted category from the list, once confirmed', async () => {
    fakeApi({
      'GET /room-categories': [{ status: 200, body: [standard, suite] }, { status: 200, body: [standard] }],
      'DELETE /room-categories/Svit': { status: 204 },
    });
    renderPage(<RoomCategories />);
    await userEvent.click(await buttonIn('Svit', 'Ta bort'));
    await answer('Ta bort kategori', 'OK');
    await waitFor(() => expect(screen.queryByRole('row', { name: /^Svit/ })).not.toBeInTheDocument());
  });

  it('cannot delete a category a room has, and says why', async () => {
    fakeApi({ 'GET /room-categories': { status: 200, body: [{ ...standard, hasRooms: true }] } });
    renderPage(<RoomCategories />);
    const remove = await buttonIn('Standard', 'Ta bort');
    expect(remove).toBeDisabled();
    expect(remove).toHaveAccessibleDescription('Kategorin har rum.');
  });

  it('says above the categories that the last category was not deleted', async () => {
    fakeApi({
      'GET /room-categories': { status: 200, body: [standard] },
      'DELETE /room-categories/Standard': { status: 409, body: { code: 'RoomCategoryIsLast' } },
    });
    renderPage(<RoomCategories />);
    await userEvent.click(await buttonIn('Standard', 'Ta bort'));
    await answer('Ta bort kategori', 'OK');
    expect(await screen.findByRole('alert')).toHaveTextContent('Hotellet måste ha minst en kategori.');
  });
});
