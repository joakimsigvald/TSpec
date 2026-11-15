import { vi } from 'vitest';

type Reply = { status: number; body?: unknown };

export function fakeApi(replies: Record<string, Reply>) {
  const fetch = vi.fn(async (path: string, init?: RequestInit) => {
    const request = `${init?.method ?? 'GET'} ${path}`;
    const reply = replies[request];
    if (!reply)
      throw new Error(`The fake API has no reply for ${request}`);

    return toResponse(reply);
  });
  vi.stubGlobal('fetch', fetch);
  return fetch;
}

const toResponse = ({ status, body }: Reply) =>
  body === undefined ? new Response(null, { status }) : Response.json(body, { status });
