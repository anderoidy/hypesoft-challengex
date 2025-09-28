'use client';
import React, { createContext, useContext, useEffect, useState, ReactNode } from 'react';
import { authService, User, LoginCredentials } from '@/services/authService';

// Types
interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (credentials: LoginCredentials) => Promise<void>;
  logout: () => Promise<void>;
  hasRole: (role: string) => boolean;
}

// Context
const AuthContext = createContext<AuthContextType | undefined>(undefined);

// Provider Props
interface AuthProviderProps {
  children: ReactNode;
}

// Auth Provider
export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Check authentication on mount
  useEffect(() => {
    checkAuth();
  }, []);

  const checkAuth = async () => {
    try {
      setIsLoading(true);
      
      if (authService.isAuthenticated()) {
        const currentUser = authService.getCurrentUser();
        setUser(currentUser);
        
        // Test if token is still valid with backend
        try {
          await authService.testAuth();
        } catch (error) {
          // Token invalid, clear auth
          await logout();
        }
      }
    } catch (error) {
      console.error('Auth check failed:', error);
      await logout();
    } finally {
      setIsLoading(false);
    }
  };

  const login = async (credentials: LoginCredentials) => {
    try {
      setIsLoading(true);
      
      const response = await authService.login(credentials);
      const currentUser = authService.getCurrentUser();
      
      setUser(currentUser);
      
      console.log('✅ Login successful:', currentUser);
    } catch (error: any) {
      console.error('❌ Login failed:', error);
      throw new Error(
        error?.response?.data?.message || 
        'Falha no login. Verifique suas credenciais.'
      );
    } finally {
      setIsLoading(false);
    }
  };

  const logout = async () => {
    try {
      setIsLoading(true);
      await authService.logout();
    } catch (error) {
      console.error('Logout error:', error);
    } finally {
      setUser(null);
      setIsLoading(false);
    }
  };

  const hasRole = (role: string): boolean => {
    return authService.hasRole(role);
  };

  const value: AuthContextType = {
    user,
    isAuthenticated: !!user && authService.isAuthenticated(),
    isLoading,
    login,
    logout,
    hasRole,
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
}

// Custom hook
export function useAuth(): AuthContextType {
  const context = useContext(AuthContext);
  
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  
  return context;
}

// Export types
export type { AuthContextType };
