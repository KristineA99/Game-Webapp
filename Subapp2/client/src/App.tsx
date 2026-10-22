import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import Container from 'react-bootstrap/Container';
import HomePage from './home/HomePage';
import QuestionListPage from './questions/QuestionListPage';
import NavMenu from './shared/NavMenu';
import './App.css';

// Holds the cache for all data fetched with useQuery
const queryClient = new QueryClient();

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <Container>
        <NavMenu />
        <Router>
          <Routes>
            <Route path="/" element={<HomePage />} />
            <Route path="/questions" element={<QuestionListPage />} />
            {/* Any unknown URL redirects to the home page */}
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </Router>
      </Container>
    </QueryClientProvider>
  );
}