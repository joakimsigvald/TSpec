# MyHotel

**Purpose:** what MyHotel is, what it does as a hotel, and how to run it, for a human meeting it for
the first time.

A small hotel-management application: an ASP.NET Core API with a React UI. It is the reference
application TSpec is developed against, not a shipped product: driven out entirely by
specifications, it is the proving ground for specification generation.

## What it does

The hotel keeps its rooms, each with a room number, a number of beds and a room category, and takes
bookings: a room held for a guest from an arrival date to a departure date. The hotel numbers each
booking itself and refuses one that overlaps another booking of the same room. A booking can be
cancelled while it is open; cancelled and past bookings are kept as a record. Rooms can be added and
changed, and deleted once they have no open bookings; a room's open bookings can be cancelled all at
once. The UI is in Swedish.

The domain's terms are defined in [GLOSSARY.md](GLOSSARY.md). What exactly the application does is
stated by its generated specifications: [the HTTP contract](Host.Spec/_specification/README.md) and
[the domain rules](Core.Spec/_specification/README.md). What comes next: [BACKLOG.md](BACKLOG.md).

It is layered per Neat, with Web as a client of the API: how and why is in [CLAUDE.md](CLAUDE.md),
and how the code is written in [CODING-STANDARD.md](CODING-STANDARD.md).

## Running it

In Visual Studio, set `Host` and `Web` as startup projects; Visual Studio keeps that choice in
`MyHotel.slnLaunch.user`, which is not under source control.

From the command line, start the API from its own folder, where it keeps its data as JSON files.
It serves [Scalar](https://scalar.com) at `http://localhost:5167/scalar`, an interactive UI over its
OpenAPI document.

```bash
cd SampleProjects/MyHotel/Host
```

```bash
dotnet run
```

Then start the UI at `http://localhost:5173`, after `npm --prefix SampleProjects/MyHotel/Web install`
the first time:

```bash
npm --prefix SampleProjects/MyHotel/Web run dev
```

The data is shared: what the page shows is the product owner's, and what is added or removed while
trying the UI stays.

The specifications run with `dotnet test`, and the UI's tests with npm: see
[TESTING.md](TESTING.md).
