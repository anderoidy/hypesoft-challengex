import { api } from './api';

// Types
interface LoginCredentials {
  username: string;
  password: string;
}

interface AuthResponse {
  access_token: string;
  refresh_token: string;
  expires_in: number;
  token_type: string;
  scope: string;
}

interface RefreshTokenRequest {
  refreshToken: string;
}

interface User {
  id: string;
  username: string;
  name: string;
  email: string;
  roles: string[];
}

// Auth Service
export const authService = {
  // Login
  async login(credentials: LoginCredentials): Promise<AuthResponse> {
    const response = await api.post<AuthResponse>('/Auth/login', credentials);
    
    // Save tokens
    localStorage.setItem('accessToken', response.data.access_token);
    localStorage.setItem('refreshToken', response.data.refresh_token);
    
    return response.data;
  },

  // Refresh Token
  async refreshToken(refreshToken: string): Promise<AuthResponse> {
    const response = await api.post<AuthResponse>('/Auth/refresh-token', {
      refreshToken
    });
    
    // Update tokens
    localStorage.setItem('accessToken', response.data.access_token);
    localStorage.setItem('refreshToken', response.data.refresh_token);
    
    return response.data;
  },

  // Logout
  async logout(): Promise<void> {
    try {
      await api.post('/Auth/logout');
    } finally {
      // Clear tokens regardless of API response
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('user');
    }
  },

  // Test Auth
  async testAuth(): Promise<any> {
    const response = await api.get('/Auth/test');
    return response.data;
  },

  // Get current user from token
  getCurrentUser(): User | null {
    const token = localStorage.getItem('accessToken');
    if (!token) return null;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      
      return {
        id: payload.sub,
        username: payload.preferred_username,
        name: payload.name,
        email: payload.email,
        roles: payload.roles || []
      };
    } catch (error) {
      console.error('Error parsing token:', error);
      return null;
    }
  },

  // Check if user is logged in
  isAuthenticated(): boolean {
    const token = localStorage.getItem('accessToken');
    if (!token) return false;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const currentTime = Date.now() / 1000;
      
      return payload.exp > currentTime;
    } catch {
      return false;
    }
  },

  // Get token
  getToken(): string | null {
    return localStorage.getItem('accessToken');
  },

  // Check if user has specific role
  hasRole(role: string): boolean {
    const user = this.getCurrentUser();
    return user?.roles.includes(role) || false;
  }
};

export type { LoginCredentials, AuthResponse, User };
