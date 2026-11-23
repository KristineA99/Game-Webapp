// Account information returned by GET /api/auth/me.
export interface AuthUser {
  nickname: string;
  roles: string[];
}

// Credentials sent to POST /api/auth/login.
export interface LoginRequest {
  email: string;
  password: string;
  rememberMe: boolean;
}

// Registration cannot include roles; permissions are assigned by the server.
export interface RegisterRequest {
  email: string;
  nickname: string;
  password: string;
  confirmPassword: string;
}