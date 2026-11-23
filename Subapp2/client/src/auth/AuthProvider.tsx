import type { ReactNode } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { AuthContext } from './AuthContext';
import * as AuthService from './AuthService';
import type { AuthUser, LoginRequest } from '../types/auth';

const userQueryKey = ['auth', 'user'] as const;

export default function AuthProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient();

  const userQuery = useQuery<AuthUser | null, Error>({
    queryKey: userQueryKey,
    queryFn: AuthService.fetchCurrentUser,
    retry: false,
  });

  async function refreshUser(): Promise<void> {
    // Propagate failures so the calling form can display useful feedback.
    await userQuery.refetch({ throwOnError: true });
  }

  async function login(model: LoginRequest): Promise<void> {
    await AuthService.login(model);

    // Load the server-confirmed nickname and roles after the cookie is issued.
    await refreshUser();
  }

  async function logout(): Promise<void> {
    await AuthService.logout();

    // Stop pending requests and discard data belonging to the previous login.
    await queryClient.cancelQueries();
    queryClient.removeQueries({
      predicate: query => query.queryKey[0] !== 'auth',
    });

    queryClient.setQueryData<AuthUser | null>(userQueryKey, null);
  }

  return (
    <AuthContext.Provider
      value={{
        // Hide protected UI if the current account cannot be verified.
        user: userQuery.isError ? null : userQuery.data ?? null,
        isLoading: userQuery.isPending,
        error: userQuery.error?.message ?? null,
        login,
        logout,
        refreshUser,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}