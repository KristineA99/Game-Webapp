import type { AuthUser, LoginRequest, RegisterRequest } from '../types/auth';

const API_URL = import.meta.env.VITE_API_URL;

export async function fetchCurrentUser(): Promise<AuthUser | null> {
    const response = await fetch(`${API_URL}/api/auth/me`, {
        // The frontend and API use different ports, so explicitly include cookies.
        credentials: 'include',
    });

    // An anonymous or expired session is an expected state, not a server failure.
    if (response.status === 401) {
        return null;
    }

    if (!response.ok) {
        throw new Error('Unable to load your account. Please try again later.');
    }

    return response.json();
}

// Stores the API's antiforgery cookie through the browser and returns its matching request token.
export async function fetchCsrfToken(): Promise<string> {
    const response = await fetch(`${API_URL}/api/auth/csrf`, {
        credentials: 'include',
        cache: 'no-store',
    });

    if (!response.ok) {
        throw new Error('Unable to prepare your request. Please try again.');
    }

    const data: { token?: string } = await response.json();

    if (!data.token) {
        throw new Error('The server did not provide a request token.');
    }

    return data.token;
}

export async function login(model: LoginRequest): Promise<void> {
    const token = await fetchCsrfToken();

    const response = await fetch(`${API_URL}/api/auth/login`, {
        method: 'POST',
        credentials: 'include',
        headers: {
            'Content-Type': 'application/json',
            'X-CSRF-TOKEN': token,
        },
        body: JSON.stringify(model),
    });

    if (response.status === 401) {
        throw new Error('Invalid email or password.');
    }

    if (!response.ok) {
        throw new Error('Unable to log in. Please try again.');
    }

    // Successful login returns 204 No Content, so there is no JSON to read.
}

export async function logout(): Promise<void> {
    // Fetch a token for the currently logged-in identity.
    const token = await fetchCsrfToken();

    const response = await fetch(`${API_URL}/api/auth/logout`, {
        method: 'POST',
        credentials: 'include',
        headers: {
            'X-CSRF-TOKEN': token,
        },
    });

    // An expired session is already logged out.
    if (response.status === 401) {
        return;
    }

    if (!response.ok) {
        throw new Error('Unable to log out. Please try again.');
    }

    // Successful logout returns 204 No Content.
}

export async function register(model: RegisterRequest): Promise<void> {
    const token = await fetchCsrfToken();

    const response = await fetch(`${API_URL}/api/auth/register`, {
        method: 'POST',
        credentials: 'include',
        headers: {
            'Content-Type': 'application/json',
            'X-CSRF-TOKEN': token,
        },
        body: JSON.stringify(model),
    });

    if (response.status === 400) {
        const problem: {
            errors?: Record<string, string[]>;
        } = await response.json();

        // Include field validation and Identity errors, such as a weak password.
        const messages = Object.values(problem.errors ?? {}).flat();

        throw new Error(
            messages.length > 0
                ? messages.join(' ')
                : 'Registration could not be completed. Please check your details.',
        );
    }

    if (!response.ok) {
        throw new Error('Unable to create your account. Please try again later.');
    }
}