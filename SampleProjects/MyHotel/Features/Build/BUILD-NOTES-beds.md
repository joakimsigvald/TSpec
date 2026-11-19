# Beds per room — build notes

Built: `Core.Spec`, `Host.Spec`, `Architecture.Spec` and Web's tests are green.

Deviations from the design:

- **Host maps `GET /rules`.** The design builds in Contract, Core, Entry and Web, but every endpoint
  is mapped in `Host/Program.cs`, so the new one takes a line there.
- **The refusal shows under "Sängar"** in the add-room form, where the design only gives it its
  text: CODING-STANDARD-FRONTEND.md 6b puts a failure under the field its code concerns.
