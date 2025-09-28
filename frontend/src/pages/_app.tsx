import { ToastProvider } from '@radix-ui/react-toast';
import type { AppProps } from 'next/app';
import { AuthProvider } from '@/contexts/AuthContext';

import '../app/globals.css';

function MyApp({ Component, pageProps }: AppProps) {
  return (
    <AuthProvider>
      <ToastProvider>
        <Component {...pageProps} />
      </ToastProvider>
    </AuthProvider>
  );
}

export default MyApp;
