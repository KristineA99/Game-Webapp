import { useState } from 'react';
import type { FormEvent } from 'react';
import { Navigate, useNavigate } from 'react-router-dom';
import { useAuth } from './AuthContext';

export default function LoginPage() {
    const { user, isLoading, login } = useAuth();
    const navigate = useNavigate();

    const [error, setError] = useState<string | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);

    async function handleSubmit(event: FormEvent<HTMLFormElement>) {
        event.preventDefault();

        // Read the form before awaiting any requests.
        const formData = new FormData(event.currentTarget);

        setError(null);
        setIsSubmitting(true);

        try {
            await login({
                email: String(formData.get('email') ?? ''),
                password: String(formData.get('password') ?? ''),
                rememberMe: formData.get('rememberMe') === 'on',
            });

            navigate('/', { replace: true });
        } catch (error) {
            setError(
                error instanceof Error ? error.message : 'Unable to log in.',
            );
        } finally {
            setIsSubmitting(false);
        }
    }

    if (isLoading) {
        return <p>Checking your account...</p>;
    }

    // Already authenticated users do not need to submit the login form again.
    if (user) {
        return <Navigate to="/" replace />;
    }

    return (
        <div className="mx-auto" style={{ maxWidth: '450px' }}>
            <h1>Log in</h1>

            {error && <div className="alert alert-danger" role="alert">{error}</div>}

            <form onSubmit={handleSubmit}>
                <div className="mb-3">
                    <label htmlFor="email" className="form-label">Email</label>
                    <input
                        id="email"
                        name="email"
                        type="email"
                        className="form-control"
                        autoComplete="username"
                        required
                    />
                </div>

                <div className="mb-3">
                    <label htmlFor="password" className="form-label">Password</label>
                    <input
                        id="password"
                        name="password"
                        type="password"
                        className="form-control"
                        autoComplete="current-password"
                        required
                    />
                </div>

                <div className="form-check mb-3">
                    <input
                        id="rememberMe"
                        name="rememberMe"
                        type="checkbox"
                        className="form-check-input"
                    />
                    <label htmlFor="rememberMe" className="form-check-label">
                        Remember me
                    </label>
                </div>

                <button
                    type="submit"
                    className="btn btn-primary"
                    disabled={isSubmitting}
                >
                    {isSubmitting ? 'Logging in...' : 'Log in'}
                </button>
            </form>
        </div>
    );
}