export const get = async <T>(path: string): Promise<T> => (await call('GET', path)).json();

export const post = async <T>(path: string, body: unknown): Promise<T> => (await call('POST', path, body)).json();

export const put = async <T>(path: string, body: unknown): Promise<T> => (await call('PUT', path, body)).json();

export const remove = async (path: string): Promise<void> => {
  await call('DELETE', path);
};

async function call(method: string, path: string, body?: unknown): Promise<Response> {
  const response = await fetch(path, {
    method,
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  });
  if (!response.ok)
    throw new Error(await errorMessage(response));

  return response;
}

async function errorMessage(response: Response): Promise<string> {
  const body = await response.json().catch(() => undefined);
  return body?.error ?? `${response.status} ${response.statusText}`;
}
