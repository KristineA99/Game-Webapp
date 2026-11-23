import { useState } from 'react';
import { Alert, Button, Nav, Navbar } from 'react-bootstrap';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

export default function NavMenu() {
  const { user, isLoading, error: accountError, logout } = useAuth();
  const navigate = useNavigate();

  const [logoutError, setLogoutError] = useState<string | null>(null);
  const [isLoggingOut, setIsLoggingOut] = useState(false);

  const isAdmin = user?.roles.includes('Admin') === true;

  async function handleLogout() {
    setLogoutError(null);
    setIsLoggingOut(true);

    try {
      await logout();

      // Leave any management page after the login cookie is removed.
      navigate('/', { replace: true });
    } catch (error) {
      setLogoutError(
        error instanceof Error ? error.message : 'Unable to log out.',
      );
    } finally {
      setIsLoggingOut(false);
    }
  }

  return (
    <>
      <Navbar expand="lg">
        <Navbar.Brand as={Link} to="/">Monster Munch</Navbar.Brand>
        <Navbar.Toggle aria-controls="basic-navbar-nav" />

        <Navbar.Collapse id="basic-navbar-nav">
          <Nav className="me-auto">
            <Nav.Link as={Link} to="/">Home</Nav.Link>

            {/* API authorization still enforces access independently of this link. */}
            {isAdmin && (
              <Nav.Link as={Link} to="/admin">Admin Dashboard</Nav.Link>
            )}
          </Nav>

          <Nav className="align-items-lg-center gap-2">
            {isLoading && <Navbar.Text>Checking account...</Navbar.Text>}

            {!isLoading && !accountError && !user && (
              <Nav.Link as={Link} to="/login">Log in</Nav.Link>
            )}

            {!isLoading && user && (
              <>
                <Navbar.Text>Hello {user.nickname}!</Navbar.Text>
                <Button
                  variant="outline-secondary"
                  onClick={handleLogout}
                  disabled={isLoggingOut}
                >
                  {isLoggingOut ? 'Logging out...' : 'Log out'}
                </Button>
              </>
            )}
          </Nav>
        </Navbar.Collapse>
      </Navbar>

      {accountError && (
        <Alert variant="warning">
          {accountError}
          <Button
            variant="link"
            onClick={() => window.location.reload()}
          >
            Refresh page
          </Button>
        </Alert>
      )}

      {logoutError && <Alert variant="danger">{logoutError}</Alert>}
    </>
  );
}