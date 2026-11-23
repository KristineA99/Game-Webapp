import { Link, Navigate, Outlet } from 'react-router-dom';
import { useAuth } from './AuthContext';

export default function AdminRoute() {
    const { user, isLoading, error } = useAuth();

    // Wait for the account check before deciding whether access is allowed.
    if (isLoading) {
        return <p>Checking your account...</p>;
    }

    if (error) {
        return <p role="alert">{error} Please refresh the page to try again.</p>;
    }

    if (!user) {
        return <Navigate to="/login" replace />;
    }

    if (!user.roles.includes('Admin')) {
        return (
            <div>
                <h1>Access denied</h1>
                <p>Only administrators can access management pages.</p>
                <Link to="/">Back to Home</Link>
            </div>
        );
    }

    // Render the requested management page only after Admin access is confirmed.
    return <Outlet />;
}