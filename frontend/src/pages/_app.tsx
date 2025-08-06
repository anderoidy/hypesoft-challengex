import { ToastProvider } from '@radix-ui/react-toast';
import type { AppProps } from 'next/app';
//import '../styles/globals.css'; // ajuste o caminho conforme necessário

import '../app/globals.css'; // ajuste o caminho conforme necessário

function MyApp({ Component, pageProps }: AppProps) {
  return (
    <ToastProvider>
      <Component {...pageProps} />
    </ToastProvider>
  );
}

export default MyApp;