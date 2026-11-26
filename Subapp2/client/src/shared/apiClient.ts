import { fetchCsrfToken } from '../auth/AuthService';

const API_URL = import.meta.env.VITE_API_URL;

// Reads the response. Throws a readable error if the request failed.
async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    throw new Error(await readErrorMessage(response));
  }

  // 204 No Content (for example after delete) has no body to read.
  if (response.status === 204) {
    return undefined as T;
  }

  return response.json();
}

// Finds the best error message the API sent back.
async function readErrorMessage(response: Response): Promise<string> {
  if (response.status === 401) return 'You must log in first.';
  if (response.status === 403) return 'You do not have access to this.';

  const text = await response.text();
  try {
    // Validation errors arrive as JSON: { errors: { Text: ["..."] } }
    const problem: { errors?: Record<string, string[]> } = JSON.parse(text);
    const messages = Object.values(problem.errors ?? {}).flat();
    if (messages.length > 0) return messages.join(' ');
  } catch {
    // Not JSON: the API sent plain text, for example "Question not found".
  }
  return text || `Request failed (status ${response.status})`;
}

// GET: read data. No CSRF token needed.
export async function apiGet<T>(path: string): Promise<T> {
  const response = await fetch(`${API_URL}${path}`, {
    credentials: 'include', // send the login cookie
  });
  return handleResponse<T>(response);
}

// POST, PUT and DELETE: change data. The API demands a CSRF token.
export async function apiSend<T>(
  method: 'POST' | 'PUT' | 'DELETE',
  path: string,
  body?: unknown,
): Promise<T> {
  const token = await fetchCsrfToken();

  const response = await fetch(`${API_URL}${path}`, {
    method,
    credentials: 'include',
    headers: {
      'Content-Type': 'application/json',
      'X-CSRF-TOKEN': token,
    },
    body: body === undefined ? undefined : JSON.stringify(body),
  });
  return handleResponse<T>(response);
}