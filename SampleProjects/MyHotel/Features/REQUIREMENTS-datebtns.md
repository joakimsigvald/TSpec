# Date buttons when booking

The booking form gets buttons under its date fields, to set a common period in one click.

- Under Från: "Idag" and "Imorgon", setting Från to that date.
- Under Till: "1 natt", "2 nätter" and "3 nätter", setting Till to that many nights after Från.
  When Från is empty, it is set to today first, so a booking for tonight is one click on "1 natt".
- A button whose date matches the field shows as pressed, in a darker colour, and cannot be
  clicked, whether it was clicked or the date was entered by hand.
- When Från changes after Till was set with a button, Till moves with it and keeps the length.
  When Till was entered by hand, it stays, and the buttons show the new length.
- After a booking the form is empty again, so no button shows as pressed.
