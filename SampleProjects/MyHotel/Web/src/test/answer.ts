import { screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';

export const dialogNamed = (caption: string) => screen.findByRole('dialog', { name: caption });

export async function answer(caption: string, button: 'OK' | 'Avbryt') {
  await userEvent.click(within(await dialogNamed(caption)).getByRole('button', { name: button }));
}
