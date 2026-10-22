import { Nav, Navbar } from 'react-bootstrap';

export default function NavMenu() {
  return (
    <Navbar expand="lg">
      <Navbar.Brand href="/">Quiz Game</Navbar.Brand>
      <Navbar.Toggle aria-controls="basic-navbar-nav" />
      <Navbar.Collapse id="basic-navbar-nav">
        <Nav className="me-auto">
          <Nav.Link href="/">Home</Nav.Link>
          <Nav.Link href="/questions">Questions</Nav.Link>
        </Nav>
      </Navbar.Collapse>
    </Navbar>
  );
}