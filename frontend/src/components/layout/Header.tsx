'use client';
import { ThemeToggle } from "@/components/theme-toggle"
import { useRouter } from 'next/navigation';
import React from 'react';
import { 
  FiBell, 
  FiLogOut,
  FiMenu, 
  FiSearch, 
  FiSettings,
  FiUser
} from 'react-icons/fi';

import { useAuth } from '@/contexts/AuthContext';

import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Input } from '@/components/ui/input';
import { cn } from '@/lib/utils';

interface HeaderProps {
  onMenuClick: () => void;
  className?: string;
}

export const Header: React.FC<HeaderProps> = ({ onMenuClick, className }) => {
  const { user, logout } = useAuth();
  const router = useRouter();

  const handleLogout = async () => {
    try {
      await logout();
      router.push('/login');
    } catch (error) {
      console.error('Erro no logout:', error);
      router.push('/login');
    }
  };

  return (
    <header className={cn(
      "sticky top-0 z-40 flex h-16 items-center gap-4 border-b bg-background px-4 sm:px-6 lg:px-8",
      className
    )}>
      <div className="flex items-center gap-4">
        {/* Menu Button para Mobile */}
        <Button
          variant="ghost"
          size="icon"
          className="lg:hidden"
          onClick={onMenuClick}
          aria-label="Toggle menu"
        >
          <FiMenu className="size-5 text-gray-700 dark:text-gray-200" />
        </Button>
      </div>

      <div className="flex flex-1 items-center justify-between gap-4">
        {/* Search Bar */}
        <div className="relative flex-1 md:max-w-md">
          <div className="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3">
            <FiSearch className="size-4 text-muted-foreground" />
          </div>
          <Input
            type="search"
            placeholder="Search..."
            className="w-full rounded-lg bg-background pl-9"
          />
        </div>

        <div className="flex items-center gap-2">
          {/* Theme Toggle usando componente */}
          <ThemeToggle />

          {/* Notifications */}
          <Button variant="ghost" size="icon" className="relative">
            <div className="absolute right-2 top-2 size-2 rounded-full bg-primary"></div>
            <FiBell className="size-5 text-gray-700 dark:text-gray-200" />
          </Button>

          {/* User Menu */}
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" className="size-10 rounded-full p-0">
                <Avatar className="size-8">
                  <AvatarImage src="/avatars/01.png" alt="User" />
                  <AvatarFallback className="bg-primary text-primary-foreground">
                    {user?.name?.charAt(0)?.toUpperCase() || <FiUser className="size-4" />}
                  </AvatarFallback>
                </Avatar>
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent className="w-56" align="end" forceMount>
              <DropdownMenuLabel className="font-normal">
                <div className="flex flex-col space-y-1">
                  <p className="text-sm font-medium leading-none">
                    {user?.name || 'Admin User'}
                  </p>
                  <p className="text-xs leading-none text-muted-foreground">
                    {user?.email || 'admin@hypesoft.com'}
                  </p>
                </div>
              </DropdownMenuLabel>
              <DropdownMenuSeparator />
              <DropdownMenuItem className="cursor-pointer">
                <FiUser className="mr-2 size-4" />
                <span>Perfil</span>
              </DropdownMenuItem>
              <DropdownMenuItem className="cursor-pointer">
                <FiSettings className="mr-2 size-4" />
                <span>Configurações</span>
              </DropdownMenuItem>
              <DropdownMenuSeparator />
              <DropdownMenuItem 
                className="cursor-pointer text-destructive"
                onClick={handleLogout}
              >
                <FiLogOut className="mr-2 size-4" />
                <span>Sair</span>
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
      </div>
    </header>
  );
};
