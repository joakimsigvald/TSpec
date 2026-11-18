# MyHotel — testing the frontend

Web is tested two ways: by its Vitest tests, and by running the application and using the page.

## Vitest

Testing Library with the API faked (`src/test/fakeApi.ts`). `dotnet test` does not run them.

```bash
npm --prefix SampleProjects/MyHotel/Web test
```

## Running it

Host serves the API on `http://localhost:5167`; Web's dev server serves the page on
`http://localhost:5173` and forwards `/rooms` and `/bookings` to Host. Start both, Host from its
own folder, since that is where its data files are:

```bash
cd SampleProjects/MyHotel/Host
```

```bash
dotnet run
```

```bash
npm --prefix SampleProjects/MyHotel/Web run dev
```

The data is shared: Host keeps it in `Host/rooms.json` and `Host/bookings.json`, so the rooms and
bookings on the page are the PO's, and what is added or removed while trying the UI stays.
