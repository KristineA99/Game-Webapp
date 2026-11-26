import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import Container from 'react-bootstrap/Container';
import HomePage from './home/HomePage';
import QuestionListPage from './questions/QuestionListPage';
import NavMenu from './shared/NavMenu';
import './App.css';
import AuthProvider from './auth/AuthProvider';
import LoginPage from './auth/LoginPage';
import AdminRoute from './auth/AdminRoute';
import AdminDashboardPage from './admin/AdminDashboardPage';
import RegisterPage from './auth/RegisterPage';
import CategoryListPage from './categories/CategoryListPage';
import CategoryCreatePage from './categories/CategoryCreatePage';
import CategoryEditPage from './categories/CategoryEditPage';

// Holds the cache for all data fetched with useQuery
const queryClient = new QueryClient();

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <Router>
          <Container>
            <NavMenu />
            <Routes>
              <Route path="/" element={<HomePage />} />
              <Route path="/login" element={<LoginPage />} />
              <Route path="/register" element={<RegisterPage />} />
              {/* All routes nested here require a confirmed Admin account. */}
              {/* The dashboard and its management pages share the Admin guard. */}
              <Route path="/admin" element={<AdminRoute />}>
                <Route index element={<AdminDashboardPage />} />
                <Route path="questions" element={<QuestionListPage />} />
                <Route path="categories" element={<CategoryListPage />} />
                <Route path="categories/create" element={<CategoryCreatePage />} />
                <Route path="categories/edit/:id" element={<CategoryEditPage />} />
              </Route>
              {/* Any unknown URL redirects to the home page */}
              <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
          </Container>
        </Router>
      </AuthProvider>
    </QueryClientProvider>
  );
}