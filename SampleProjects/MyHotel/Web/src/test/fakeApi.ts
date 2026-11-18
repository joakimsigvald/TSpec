import { vi } from 'vitest';

type Reply = { status: number; body?: unknown };

/**
 * A list of replies answers successive requests in order, its last answering every one after: a
 * list the page reloads after a change is declared as it is before and after.
 */
export function fakeApi(replies: Record<string, Reply | Reply[]>) {
  const answered: Record<string, number> = {};
  const fetch = vi.fn(async (path: string, init?: RequestInit) => {
    const request = `${init?.method ?? 'GET'} ${path}`;
    const declared = replies[request];
    if (!declared)
      throw new Error(`The fake API has no reply for ${request}`);

    const sequence = Array.isArray(declared) ? declared : [declared];
    const count = answered[request] ?? 0;
    answered[request] = count + 1;
    return toResponse(sequence[Math.min(count, sequence.length - 1)]);
  });
  vi.stubGlobal('fetch', fetch);
  return fetch;
}

const toResponse = ({ status, body }: Reply) =>
  body === undefined ? new Response(null, { status }) : Response.json(body, { status });
