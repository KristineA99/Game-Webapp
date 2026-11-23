import { useState } from 'react';
import type { ComponentProps } from 'react';
import { Link, Navigate } from 'react-router-dom';
import { useAuth } from './AuthContext';
import { register } from './AuthService';

export default function RegisterPage() {
    const { user, isLoading } = useAuth();

    const [error, setError] = useState<string | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [isRegistered, setIsRegistered] = useState(false);

    const handleSubmit: ComponentProps<'form'>['onSubmit'] = async (event) => {
        event.preventDefault();

        const formData = new FormData(event.currentTarget);
        const password = String(formData.get('password') ?? '');
        const confirmPassword = String(formData.get('confirmPassword') ?? '');

        setError(null);

        // Give immediate feedback; the API also validates password confirmation.
        if (password !== confirmPassword) {
            setError('The passwords do not match.');
            return;
        }

        setIsSubmitting(true);

        try {
            await register({
                email: String(formData.get('email') ?? ''),
                nickname: String(formData.get('nickname') ?? ''),
                password,
                confirmPassword,
            });

            setIsRegistered(true);
        } catch (error) {
            setError(
                error instanceof Error ? error.message : 'Unable to create your account.',
            );
        } finally {
            setIsSubmitting(false);
        }
    };

    if (isLoading) {
        return <p>Checking your account...</p>;
    }

    if (user) {
        return <Navigate to="/" replace />;
    }

    if (isRegistered) {
        return (
            <div className="alert alert-success" role="status">
                Account created successfully. <Link to="/login">Log in</Link> to continue.
            </div>
        );
    }

    return (
        <div className="mx-auto" style={{ maxWidth: '450px' }}>
            <h1>Create account</h1>

            {error && <div className="alert alert-danger" role="alert">{error}</div>}

            <form onSubmit={handleSubmit}>
                <div className="mb-3">
                    <label htmlFor="email" className="form-label">Email</label>
                    <input
                        id="email"
                        name="email"
                        type="email"
                        className="form-control"
                        autoComplete="email"
                        required
                    />
                </div>

                <div className="mb-3">
                    <label htmlFor="nickname" className="form-label">Nickname</label>
                    <input
                        id="nickname"
                        name="nickname"
                        className="form-control"
                        autoComplete="username"
                        minLength={3}
                        maxLength={20}
                        pattern="[a-zA-Z0-9_.\-]+"
                        title="Use 3–20 letters, numbers, underscores, dots, or hyphens."
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
                        autoComplete="new-password"
                        minLength={6}
                        aria-describedby="password-help"
                        required
                    />
                    <div id="password-help" className="form-text">
                        Use at least 6 characters, including uppercase and lowercase
                        letters, a number, and a symbol.
                    </div>
                </div>

                <div className="mb-3">
                    <label htmlFor="confirmPassword" className="form-label">
                        Confirm password
                    </label>
                    <input
                        id="confirmPassword"
                        name="confirmPassword"
                        type="password"
                        className="form-control"
                        autoComplete="new-password"
                        required
                    />
                </div>

                <button
                    type="submit"
                    className="btn btn-primary"
                    disabled={isSubmitting}
                >
                    {isSubmitting ? 'Creating account...' : 'Create account'}
                </button>
            </form>

            <p className="mt-3">
                Already have an account? <Link to="/login">Log in</Link>.
            </p>
        </div>
    );
}