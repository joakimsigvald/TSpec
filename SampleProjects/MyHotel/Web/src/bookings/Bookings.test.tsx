import { screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { fakeApi } from '../test/fakeApi';
import { renderPage } from '../test/renderPage';
import { Bookings } from './Bookings';
import type { Booking, BookingAction, BookingStatus } from './bookingsApi';

const request = {
  roomNumber: '101',
  guestName: 'Smith',
  guestCount: 2,
  period: { from: '2026-08-10', to: '2026-08-12' },
};
const booking10001: Booking = {
  bookingNumber: 10001,
  info: { roomNumber: '101', guestName: 'Smith', guestCount: 2 },
  period: request.period,
  state: { status: 'Open', actions: ['Cancel'] },
};
const booking10002: Booking = {
  bookingNumber: 10002,
  info: { roomNumber: '102', guestName: 'Jones', guestCount: 1 },
  period: { from: '2026-08-11', to: '2026-08-13' },
  state: { status: 'Open', actions: ['Cancel'] },
};
const rooms = ['101', '12A', '2', '12']
  .map(roomNumber => ({ roomNumber, bedCount: 1, held: false, hasCancellableBookings: false }));
const roomsReply = { status: 200, body: rooms };

const inState = (booking: Booking, status: BookingStatus, actions: BookingAction[]): Booking =>
  ({ ...booking, state: { status, actions } });

const rowOf = (bookingNumber: number) => screen.findByRole('row', { name: new RegExp(`^${bookingNumber}`) });
const buttonsIn = (row: HTMLElement) => within(row).queryAllByRole('button').map(button => button.textContent);

const showing = (...bookings: Booking[]) => {
  fakeApi({ 'GET /rooms': roomsReply, 'GET /bookings': { status: 200, body: bookings } });
  renderPage(<Bookings />);
};

const refusedWhenClicking = async (button: string, booking: Booking, request: string, code: string) => {
  fakeApi({
    'GET /rooms': roomsReply,
    'GET /bookings': { status: 200, body: [booking] },
    [request]: { status: 409, body: { code } },
  });
  renderPage(<Bookings />);
  await userEvent.click(within(await rowOf(booking.bookingNumber)).getByRole('button', { name: button }));
};

const fillers: Record<string, () => Promise<void>> = {
  Från: () => userEvent.type(screen.getByLabelText('Från'), request.period.from),
  Till: () => userEvent.type(screen.getByLabelText('Till'), request.period.to),
  Rumsnummer: () => userEvent.selectOptions(screen.getByLabelText('Rumsnummer'), request.roomNumber),
  'Antal gäster': () => userEvent.type(screen.getByLabelText('Antal gäster'), String(request.guestCount)),
  Gäst: () => userEvent.type(screen.getByLabelText('Gäst'), request.guestName),
};
const labels = Object.keys(fillers);

const fillIn = async (filled = labels) => {
  for (const label of filled)
    await fillers[label]();
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

const openForm = () => {
  fakeApi({ 'GET /rooms': roomsReply, 'GET /bookings': { status: 200, body: [] } });
  renderPage(<Bookings />);
};

const click = (name: string) => userEvent.click(screen.getByRole('button', { name }));

const pressedButtons = () => screen.queryAllByRole('button', { pressed: true }).map(button => button.textContent);

const book = async () => {
  renderPage(<Bookings />);
  await waitFor(() => expect(offeredRooms()).toContain(request.roomNumber));
  await fillIn();
  await userEvent.click(screen.getByRole('button', { name: 'Boka' }));
};

const changeField = (label: string) => screen.getByLabelText(`${label} i bokning 10001`);

const startChanging = async () => {
  await userEvent.click(within(await rowOf(10001)).getByRole('button', { name: 'Ändra' }));
  await waitFor(() => expect(changeField('Rum')).toHaveDisplayValue('101'));
};

const changeRefusedWith = async (status: number, code: string) => {
  fakeApi({
    'GET /rooms': roomsReply,
    'GET /bookings': { status: 200, body: [booking10001] },
    'PUT /bookings/10001': { status, body: { code } },
  });
  renderPage(<Bookings />);
  await startChanging();
  await userEvent.type(changeField('Gäst'), 'son');
  await click('Spara');
};

const bookTwoNightsByButton = async () => {
  bookAccepted();
  renderPage(<Bookings />);
  await waitFor(() => expect(offeredRooms()).toContain(request.roomNumber));
  await fillIn(['Rumsnummer', 'Antal gäster', 'Gäst']);
  await click('2 nätter');
  await click('Boka');
  await rowOf(10001);
};

describe('Bookings', () => {
  it('lists the bookings', async () => {
    fakeApi({ 'GET /rooms': roomsReply, 'GET /bookings': { status: 200, body: [booking10001, booking10002] } });
    renderPage(<Bookings />);
    expect(await rowOf(10001)).toHaveTextContent('100011012Smith2026-08-102026-08-12');
    expect(await rowOf(10002)).toHaveTextContent('100021021Jones2026-08-112026-08-13');
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

  it.each(labels)('cannot book without %s', async empty => {
    openForm();
    await waitFor(() => expect(offeredRooms()).toContain(request.roomNumber));
    await fillIn(labels.filter(label => label !== empty));
    expect(screen.getByRole('button', { name: 'Boka' })).toBeDisabled();
  });

  it('cannot book again until another booking is entered', async () => {
    bookAccepted();
    await book();
    await waitFor(() => expect(screen.getByRole('button', { name: 'Boka' })).toBeDisabled());
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

  it('says under the number of guests that a booking is for at least one guest', async () => {
    await bookRefusedWith(400, 'InvalidGuestCount');
    await waitFor(() =>
      expect(screen.getByLabelText('Antal gäster'))
        .toHaveAccessibleDescription('En bokning måste gälla minst en gäst.'));
  });

  it('says under the number of guests that the room has too few beds', async () => {
    await bookRefusedWith(409, 'TooManyGuests');
    await waitFor(() =>
      expect(screen.getByLabelText('Antal gäster')).toHaveAccessibleDescription('Rummet har för få sängar.'));
  });

  it('asks for the number of guests in an empty number field', () => {
    openForm();
    const guestCount = screen.getByRole('spinbutton', { name: 'Antal gäster' });
    expect(guestCount).toHaveValue(null);
    expect(guestCount).toBeRequired();
  });

  it('puts the period first, then the room, the number of guests and the guest', () => {
    openForm();
    const fields = ['Från', 'Till', 'Rumsnummer', 'Antal gäster', 'Gäst'].map(label => screen.getByLabelText(label));
    fields.slice(1).forEach((field, index) =>
      expect(fields[index].compareDocumentPosition(field) & Node.DOCUMENT_POSITION_FOLLOWING).toBeTruthy());
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
        { status: 200, body: [inState(booking10001, 'Cancelled', []), booking10002] },
      ],
      'DELETE /bookings/10001': { status: 204 },
    });
    renderPage(<Bookings />);
    await userEvent.click(within(await rowOf(10001)).getByRole('button', { name: 'Avboka' }));
    await waitFor(async () => expect(await rowOf(10001)).toHaveTextContent(/AvbokadÄndra$/));
    expect(api).toHaveBeenCalledWith('/bookings/10001', expect.objectContaining({ method: 'DELETE' }));
  });

  it('offers no cancelling of a cancelled booking', async () => {
    showing(inState(booking10001, 'Cancelled', []));
    expect(within(await rowOf(10001)).queryByRole('button', { name: 'Avboka' })).not.toBeInTheDocument();
  });

  it('offers no cancelling of a past booking, nor calls it cancelled', async () => {
    showing(inState(booking10001, 'Past', []));
    const row = await rowOf(10001);
    expect(within(row).queryByRole('button', { name: 'Avboka' })).not.toBeInTheDocument();
    expect(row).toHaveTextContent(/2026-08-12Ändra$/);
  });

  it('offers to cancel an open booking, and to check it in when it can be', async () => {
    showing(inState(booking10001, 'Open', ['Cancel', 'CheckIn']), booking10002);
    expect(buttonsIn(await rowOf(10001))).toEqual(['Avboka', 'Checka in', 'Ändra']);
    expect(buttonsIn(await rowOf(10002))).toEqual(['Avboka', 'Ändra']);
  });

  it('checks in a booking, and shows it checked in', async () => {
    const api = fakeApi({
      'GET /rooms': roomsReply,
      'GET /bookings': [
        { status: 200, body: [inState(booking10001, 'Open', ['Cancel', 'CheckIn'])] },
        { status: 200, body: [inState(booking10001, 'CheckedIn', [])] },
      ],
      'POST /bookings/10001/check-in': { status: 204 },
    });
    renderPage(<Bookings />);
    await userEvent.click(within(await rowOf(10001)).getByRole('button', { name: 'Checka in' }));
    await waitFor(async () => expect(await rowOf(10001)).toHaveTextContent(/IncheckadÄndra$/));
    expect(api).toHaveBeenCalledWith('/bookings/10001/check-in', expect.objectContaining({ method: 'POST' }));
  });

  it('shows a checked-in booking as checked in, and offers to check it out when it can be', async () => {
    showing(inState(booking10001, 'CheckedIn', ['CheckOut']), inState(booking10002, 'CheckedIn', []));
    const checkingOut = await rowOf(10001);
    expect(checkingOut).toHaveTextContent('Incheckad');
    expect(buttonsIn(checkingOut)).toEqual(['Checka ut', 'Ändra']);
    expect(buttonsIn(await rowOf(10002))).toEqual(['Ändra']);
  });

  it('checks out a booking, and shows it checked out', async () => {
    const api = fakeApi({
      'GET /rooms': roomsReply,
      'GET /bookings': [
        { status: 200, body: [inState(booking10001, 'CheckedIn', ['CheckOut'])] },
        { status: 200, body: [inState(booking10001, 'CheckedOut', [])] },
      ],
      'POST /bookings/10001/check-out': { status: 204 },
    });
    renderPage(<Bookings />);
    await userEvent.click(within(await rowOf(10001)).getByRole('button', { name: 'Checka ut' }));
    await waitFor(async () => expect(await rowOf(10001)).toHaveTextContent(/UtcheckadÄndra$/));
    expect(buttonsIn(await rowOf(10001))).toEqual(['Ändra']);
    expect(api).toHaveBeenCalledWith('/bookings/10001/check-out', expect.objectContaining({ method: 'POST' }));
  });

  it('shows a no-show as a no-show, and offers to cancel it while it can be', async () => {
    showing(inState(booking10001, 'NoShow', ['Cancel']), inState(booking10002, 'NoShow', []));
    const cancellable = await rowOf(10001);
    expect(cancellable).toHaveTextContent('No show');
    expect(buttonsIn(cancellable)).toEqual(['Avboka', 'Ändra']);
    expect(await rowOf(10002)).toHaveTextContent(/No showÄndra$/);
    expect(buttonsIn(await rowOf(10002))).toEqual(['Ändra']);
  });

  it('says above the bookings that a booking was not checked in', async () => {
    const checkingIn = inState(booking10001, 'Open', ['CheckIn']);
    await refusedWhenClicking('Checka in', checkingIn, 'POST /bookings/10001/check-in', 'NotDueForCheckIn');
    expect(await screen.findByRole('alert')).toHaveTextContent('Bokningen kan inte checkas in.');
  });

  it('says above the bookings that a booking was not checked out', async () => {
    const checkingOut = inState(booking10001, 'CheckedIn', ['CheckOut']);
    await refusedWhenClicking('Checka ut', checkingOut, 'POST /bookings/10001/check-out', 'NotDueForCheckOut');
    expect(await screen.findByRole('alert')).toHaveTextContent('Bokningen kan inte checkas ut.');
  });

  describe('the date buttons', () => {
    beforeEach(() => {
      vi.useFakeTimers({ toFake: ['Date'] });
      vi.setSystemTime(new Date(2026, 7, 13));
    });

    afterEach(() => {
      vi.useRealTimers();
    });

    it('set the arrival to today', async () => {
      openForm();
      await click('Idag');
      expect(screen.getByLabelText('Från')).toHaveValue('2026-08-13');
      expect(pressedButtons()).toEqual(['Idag']);
    });

    it('set the arrival to tomorrow', async () => {
      openForm();
      await click('Imorgon');
      expect(screen.getByLabelText('Från')).toHaveValue('2026-08-14');
      expect(pressedButtons()).toEqual(['Imorgon']);
    });

    it('set the departure a number of nights after the arrival', async () => {
      openForm();
      await userEvent.type(screen.getByLabelText('Från'), '2026-08-20');
      await click('3 nätter');
      expect(screen.getByLabelText('Till')).toHaveValue('2026-08-23');
      expect(pressedButtons()).toEqual(['3 nätter']);
    });

    it('set the arrival to today when the nights are chosen first', async () => {
      openForm();
      await click('1 natt');
      expect(screen.getByLabelText('Från')).toHaveValue('2026-08-13');
      expect(screen.getByLabelText('Till')).toHaveValue('2026-08-14');
      expect(pressedButtons()).toEqual(['Idag', '1 natt']);
    });

    it('show as pressed when their date is typed by hand', async () => {
      openForm();
      await userEvent.type(screen.getByLabelText('Från'), '2026-08-14');
      await userEvent.type(screen.getByLabelText('Till'), '2026-08-16');
      expect(pressedButtons()).toEqual(['Imorgon', '2 nätter']);
    });

    it('cannot be clicked when pressed', async () => {
      openForm();
      await click('Idag');
      expect(screen.getByRole('button', { name: 'Idag' })).toBeDisabled();
      expect(screen.getByRole('button', { name: 'Imorgon' })).toBeEnabled();
    });

    it('move the departure with the arrival when its nights were chosen by button', async () => {
      openForm();
      await click('Idag');
      await click('2 nätter');
      await click('Imorgon');
      expect(screen.getByLabelText('Till')).toHaveValue('2026-08-16');
      expect(pressedButtons()).toEqual(['Imorgon', '2 nätter']);
    });

    it('keep a departure typed by hand when the arrival changes', async () => {
      openForm();
      await click('Idag');
      await userEvent.type(screen.getByLabelText('Till'), '2026-08-15');
      await click('Imorgon');
      expect(screen.getByLabelText('Till')).toHaveValue('2026-08-15');
      expect(pressedButtons()).toEqual(['Imorgon', '1 natt']);
    });

    it('show none as pressed after a booking', async () => {
      await bookTwoNightsByButton();
      expect(screen.getByLabelText('Från')).toHaveValue('');
      expect(pressedButtons()).toEqual([]);
    });

    it('forget the nights chosen once a booking is made', async () => {
      await bookTwoNightsByButton();
      await userEvent.type(screen.getByLabelText('Från'), '2026-08-20');
      expect(screen.getByLabelText('Till')).toHaveValue('');
    });
  });

  it('says above the bookings that a booking was not cancelled', async () => {
    await refusedWhenClicking('Avboka', booking10001, 'DELETE /bookings/10001', 'BookingNotCancellable');
    expect(await screen.findByRole('alert')).toHaveTextContent('Bokningen kan inte avbokas.');
  });

  describe('changing a booking', () => {
    it('is offered for every booking, last in its row', async () => {
      showing(booking10001, inState(booking10002, 'Cancelled', []));
      expect(buttonsIn(await rowOf(10001))).toEqual(['Avboka', 'Ändra']);
      expect(buttonsIn(await rowOf(10002))).toEqual(['Ändra']);
    });

    it("turns the row into fields holding the booking's values", async () => {
      showing(booking10001);
      await startChanging();
      expect(changeField('Rum')).toHaveDisplayValue('101');
      expect(changeField('Gäster')).toHaveValue(2);
      expect(changeField('Gäst')).toHaveValue('Smith');
      expect(changeField('Från')).toHaveValue('2026-08-10');
      expect(changeField('Till')).toHaveValue('2026-08-12');
      expect(buttonsIn(await rowOf(10001))).toEqual(['Spara', 'Avbryt']);
    });

    it('offers the rooms in alphanumeric order', async () => {
      showing(booking10001);
      await startChanging();
      expect(within(changeField('Rum')).getAllByRole('option').map(option => option.textContent))
        .toEqual(['2', '12', '12A', '101']);
    });

    it('holds the room of a booking whose room is gone', async () => {
      showing({ ...booking10001, info: { ...booking10001.info, roomNumber: '55' } });
      await userEvent.click(within(await rowOf(10001)).getByRole('button', { name: 'Ändra' }));
      expect(changeField('Rum')).toHaveDisplayValue('55');
    });

    it('cannot save until something is changed', async () => {
      showing(booking10001);
      await startChanging();
      expect(screen.getByRole('button', { name: 'Spara' })).toBeDisabled();
      await userEvent.type(changeField('Gäst'), 'son');
      expect(screen.getByRole('button', { name: 'Spara' })).toBeEnabled();
    });

    it.each(['Gäster', 'Gäst', 'Från', 'Till'])('cannot save without %s', async emptied => {
      showing(booking10001);
      await startChanging();
      await userEvent.clear(changeField(emptied));
      expect(screen.getByRole('button', { name: 'Spara' })).toBeDisabled();
    });

    it('saves the change, and shows it on the row', async () => {
      const changed = { ...booking10001, info: { ...booking10001.info, guestName: 'Jones' } };
      const api = fakeApi({
        'GET /rooms': roomsReply,
        'GET /bookings': [{ status: 200, body: [booking10001] }, { status: 200, body: [changed] }],
        'PUT /bookings/10001': { status: 200, body: changed },
      });
      renderPage(<Bookings />);
      await startChanging();
      await userEvent.clear(changeField('Gäst'));
      await userEvent.type(changeField('Gäst'), 'Jones');
      await click('Spara');
      await waitFor(async () => expect(buttonsIn(await rowOf(10001))).toEqual(['Avboka', 'Ändra']));
      expect(await rowOf(10001)).toHaveTextContent('100011012Jones2026-08-102026-08-12');
      expect(api).toHaveBeenCalledWith('/bookings/10001', expect.objectContaining({
        method: 'PUT',
        body: JSON.stringify({ ...request, guestName: 'Jones' }),
      }));
    });

    it('shows the row as it was when the change is abandoned', async () => {
      showing(booking10001);
      await startChanging();
      await userEvent.type(changeField('Gäst'), 'son');
      await click('Avbryt');
      expect(await rowOf(10001)).toHaveTextContent('100011012Smith2026-08-102026-08-12');
      expect(buttonsIn(await rowOf(10001))).toEqual(['Avboka', 'Ändra']);
      await startChanging();
      expect(changeField('Gäst')).toHaveValue('Smith');
    });

    it.each([
      ['Rum', 'RoomNotFound', 404, 'Rummet finns inte.'],
      ['Rum', 'RoomAlreadyBooked', 409, 'Rummet är redan bokat under några av nätterna.'],
      ['Gäster', 'InvalidGuestCount', 400, 'En bokning måste gälla minst en gäst.'],
      ['Gäster', 'TooManyGuests', 409, 'Rummet har för få sängar.'],
      ['Till', 'InvalidBookingPeriod', 400, 'En bokning måste gälla minst en natt.'],
    ])('says under %s that the change was refused as %s', async (field, code, status, text) => {
      await changeRefusedWith(status, code);
      await waitFor(() => expect(changeField(field)).toHaveAccessibleDescription(text));
    });

    it('says under the button that the booking no longer exists', async () => {
      await changeRefusedWith(404, 'BookingNotFound');
      await waitFor(() =>
        expect(screen.getByRole('button', { name: 'Spara' })).toHaveAccessibleDescription('Bokningen finns inte.'));
    });
  });
});
