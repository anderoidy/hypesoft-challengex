import React, { ReactNode, useState, useEffect } from 'react';
import { Inter } from 'next/font/google';
import { cn } from '@/lib/utils';
import { Sidebar } from './Sidebar';
import { Header } from './Header';
import { Footer } from './Footer';
import { Toast } from '@/components/ui/toaster';


const inter = Inter({ 
  subsets: ['latin'],
  variable: '--font-sans',
});

interface MainLayoutProps {
  children: ReactNode;
  title?: string;
  description?: string;
  className?: string;
}

export const MainLayout: React.FC<MainLayoutProps> = ({
  children,
  title = 'Hypesoft Challenge',
  description = 'Dashboard administrativo',
  className,
}) => {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [isMobile, setIsMobile] = useState(false);

  useEffect(() => {
    const handleResize = () => {
      setIsMobile(window.innerWidth < 1024);
    };

    // Set initial value
    handleResize();

    // Add event listener
    window.addEventListener('resize', handleResize);

    // Clean up
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  return (
    <div className={cn(
      'min-h-screen bg-background font-sans antialiased',
      inter.variable,
      className
    )}>
      <div className="flex h-screen overflow-hidden">
        {/* Sidebar */}
        <Sidebar 
          isOpen={sidebarOpen} 
          onClose={() => setSidebarOpen(false)} 
          isMobile={isMobile} 
        />

        {/* Main content */}
        <div className="relative flex flex-1 flex-col overflow-y-auto overflow-x-hidden">
          {/* Header */}
          <Header onMenuClick={() => setSidebarOpen(!sidebarOpen)} />

          {/* Main content */}
          <main className="flex-1 px-4 py-6 sm:px-6 lg:px-8">
            <div className="mx-auto max-w-7xl">
              {title && (
                <div className="mb-6">
                  <h1 className="text-2xl font-bold text-foreground">{title}</h1>
                  {description && (
                    <p className="mt-1 text-sm text-muted-foreground">
                      {description}
                    </p>
                  )}
                </div>
              )}
              {children}
            </div>
          </main>

          {/* Footer */}
          <Footer />
        </div>
      </div>

      {/* Toast notifications */}
      <Toast />
    </div>
  );
};
