# Date buttons when booking — design

Built in Web alone; the backend does not change.

**The booking period gets a component of its own.** Från and Till, with their buttons, move out of
the booking form into a part that keeps the two dates and whether Till was set with a button. The
form still sends whatever the two fields hold, and empties them after a booking as it does the
others.

**A button is pressed when its date is the field's.** Each button works out its date - today,
tomorrow, Från plus its nights - and compares it with the field, so a date typed by hand presses
the matching button just as a click does. A pressed button is darker and disabled.

**"Idag" is the browser's date.** The hotel's today is the server's, which the API does not offer.
At the reception desk the two are the same day; asking the server would need a new endpoint.
