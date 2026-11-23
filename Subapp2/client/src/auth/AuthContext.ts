import { createContext, useContext } from 'react';
import type { AuthUser, LoginRequest } from '../types/auth';

export interface AuthContextValue {
    user: AuthUser | null;
    isLoading: boolean;
    error: string | null;
    login: (model: LoginRequest) => Promise<void>;
    logout: () => Promise<void>;
    refreshUser: () => Promise<void>;
}

// A missing provider is different from an anonymous user.
export const AuthContext = createContext<AuthContextValue | undefined>(
    undefined,
);

export function useAuth(): AuthContextValue {
    const context = useContext(AuthContext);

    if (context === undefined) {
        throw new Error('useAuth must be used within AuthProvider.');
    }

    return context;
}