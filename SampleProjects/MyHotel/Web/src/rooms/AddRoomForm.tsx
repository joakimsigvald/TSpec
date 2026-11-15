import type { FormEvent } from 'react';
import type { Room } from './roomsApi';

type Props = { onAdd: (room: Room) => Promise<boolean> };

export function AddRoomForm({ onAdd }: Props) {
  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = event.currentTarget;
    if (await onAdd(roomIn(new FormData(form))))
      form.reset();
  };

  return (
    <form onSubmit={submit}>
      <label>
        Room number
        <input name="roomNumber" required />
      </label>
      <label>
        Beds
        <input name="bedCount" type="number" required />
      </label>
      <button>Add room</button>
    </form>
  );
}

const roomIn = (data: FormData): Room => ({
  roomNumber: String(data.get('roomNumber')),
  bedCount: Number(data.get('bedCount')),
});
