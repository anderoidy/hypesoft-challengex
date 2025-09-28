'use client';
import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/contexts/AuthContext';

export default function LoginPage() {
  const [credentials, setCredentials] = useState({
    username: '',
    password: ''
  });
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');
  
  const { login, isAuthenticated, isLoading: authLoading } = useAuth();
  const router = useRouter();

  // Proteção reversa - redireciona se já estiver logado
  useEffect(() => {
    if (!authLoading && isAuthenticated) {
      router.push('/dashboard');
    }
  }, [isAuthenticated, authLoading, router]);

  // ✅ ÚNICA CORREÇÃO: Tipagem correta do evento de submit
  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setIsLoading(true);
    setError('');

    try {
      await login(credentials);
      router.push('/dashboard');
    } catch (error) {
      setError(error instanceof Error ? error.message : 'Erro desconhecido');
    } finally {
      setIsLoading(false);
    }
  };

  // Loading enquanto verifica se já está logado
  if (authLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-gray-50 to-gray-100 dark:from-gray-900 dark:to-gray-800">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  // Se já logado, não mostra o formulário (está redirecionando)
  if (isAuthenticated) {
    return null;
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-gray-50 to-gray-100 dark:from-gray-900 dark:to-gray-800">
      <div className="w-full max-w-md mx-4">
        <div className="bg-white dark:bg-gray-800 rounded-xl shadow-2xl p-8">
          <div className="text-center mb-8">
            <div className="w-16 h-16 bg-blue-600 rounded-full flex items-center justify-center mx-auto mb-4">
              <span className="text-white font-bold text-xl">H</span>
            </div>
            <h2 className="text-3xl font-bold text-gray-900 dark:text-white">
              Hypesoft Challenge
            </h2>
            <p className="mt-2 text-gray-600 dark:text-gray-400">
              Entre com suas credenciais para acessar o sistema
            </p>
          </div>
          
          <form onSubmit={handleSubmit} className="space-y-6">
            <div>
              <label htmlFor="username" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Username
              </label>
              <input
                id="username"
                name="username"
                type="text"
                required
                value={credentials.username}
                onChange={(e) => setCredentials({...credentials, username: e.target.value})}
                className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 dark:border-gray-600 dark:bg-gray-700 dark:text-white transition-colors"
                placeholder="Digite seu username"
              />
            </div>
            
            <div>
              <label htmlFor="password" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Password
              </label>
              <input
                id="password"
                name="password"
                type="password"
                required
                value={credentials.password}
                onChange={(e) => setCredentials({...credentials, password: e.target.value})}
                className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 dark:border-gray-600 dark:bg-gray-700 dark:text-white transition-colors"
                placeholder="Digite sua senha"
              />
            </div>

            {error && (
              <div className="bg-red-50 border border-red-200 text-red-600 px-4 py-3 rounded-lg text-sm dark:bg-red-900/20 dark:border-red-800 dark:text-red-400">
                {error}
              </div>
            )}

            <button
              type="submit"
              disabled={isLoading}
              className="w-full bg-blue-600 hover:bg-blue-700 text-white font-medium py-3 px-4 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors duration-200 flex items-center justify-center"
            >
              {isLoading ? (
                <>
                  <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                  </svg>
                  Entrando...
                </>
              ) : 'Entrar no Sistema'}
            </button>
          </form>

          <div className="mt-8 p-4 bg-blue-50 dark:bg-blue-900/20 rounded-lg">
            <p className="text-sm text-blue-800 dark:text-blue-200 text-center font-medium mb-2">
              Credenciais de Demonstração:
            </p>
            <div className="text-sm text-center space-y-1">
              <p><span className="font-medium">Username:</span> <code className="bg-blue-100 dark:bg-blue-800 px-2 py-1 rounded">testuser</code></p>
              <p><span className="font-medium">Password:</span> <code className="bg-blue-100 dark:bg-blue-800 px-2 py-1 rounded">123456</code></p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}