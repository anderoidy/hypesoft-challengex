import Link from 'next/link';
import { usePathname } from 'next/navigation';
import React from 'react';
import {
  FiBox,
  FiChevronDown,
  FiChevronRight,
  FiFileText,
  FiHome,
  FiLayers,
  FiLogOut,
  FiPieChart,
  FiSettings,
  FiShoppingBag,
  FiTag,
  FiUser,
  FiUsers,
} from 'react-icons/fi';

import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Button } from '@/components/ui/button';
import { ScrollArea } from '@/components/ui/scroll-area';
import { cn } from '@/lib/utils';

export interface SidebarItem {
  title: string;
  href: string;
  icon: React.ReactNode;
  items?: SidebarItem[];
}

const sidebarItems: SidebarItem[] = [
  {
    title: 'Dashboard',
    href: '/dashboard',
    icon: <FiHome className="size-5" />,
  },
  {
    title: 'Products',
    href: '/products',
    icon: <FiShoppingBag className="size-5" />,
    items: [
      { 
        title: 'All Products', 
        href: '/products', 
        icon: <FiBox className="size-4" /> 
      },
      { 
        title: 'Categories', 
        href: '/products/categories', 
        icon: <FiLayers className="size-4" /> 
      },
      { 
        title: 'Inventory', 
        href: '/products/inventory', 
        icon: <FiTag className="size-4" /> 
      },
    ],
  },
  {
    title: 'Customers',
    href: '/customers',
    icon: <FiUsers className="size-5" />,
  },
  {
    title: 'Orders',
    href: '/orders',
    icon: <FiFileText className="size-5" />,
  },
  {
    title: 'Analytics',
    href: '/analytics',
    icon: <FiPieChart className="size-5" />,
  },
  {
    title: 'Settings',
    href: '/settings',
    icon: <FiSettings className="size-5" />,
  },
];

interface SidebarProps {
  isOpen: boolean;
  isMobile: boolean;
  onClose: () => void;
  className?: string;
}

export const Sidebar: React.FC<SidebarProps> = ({
  isOpen,
  isMobile,
  onClose,
  className,
}) => {
  const pathname = usePathname();
  const [expandedItems, setExpandedItems] = React.useState<Record<string, boolean>>({});

  const toggleItem = (title: string) => {
    setExpandedItems(prev => ({
      ...prev,
      [title]: !prev[title],
    }));
  };

  const isActive = (href: string) => {
    return pathname === href || pathname.startsWith(`${href}/`);
  };

  const renderSidebarItem = (item: SidebarItem, depth = 0) => {
    const hasChildren = item.items && item.items.length > 0;
    const isItemActive = isActive(item.href);
    const isExpanded = expandedItems[item.title];

    return (
      <div key={item.href} className="space-y-1">
        <div
          className={cn(
            'flex items-center justify-between rounded-lg px-3 py-2 text-sm font-medium transition-colors',
            isItemActive
              ? 'bg-primary/10 text-primary'
              : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground',
            depth > 0 && 'pl-8',
          )}
        >
          <Link
            href={item.href}
            className="flex flex-1 items-center gap-3"
            onClick={() => isMobile && onClose()}
          >
            <span className={cn(depth > 0 ? 'text-muted-foreground' : '')}>
              {item.icon}
            </span>
            <span>{item.title}</span>
          </Link>
          {hasChildren && (
            <Button
              variant="ghost"
              size="icon"
              className="size-6 rounded-full"
              onClick={(e) => {
                e.preventDefault();
                e.stopPropagation();
                toggleItem(item.title);
              }}
            >
              {isExpanded ? (
                <FiChevronDown className="size-4" />
              ) : (
                <FiChevronRight className="size-4" />
              )}
            </Button>
          )}
        </div>
        {hasChildren && isExpanded && (
          <div className="mt-1 space-y-1">
            {item.items?.map((child) => renderSidebarItem(child, depth + 1))}
          </div>
        )}
      </div>
    );
  };

  return (
    <aside
      className={cn(
        'fixed inset-y-0 left-0 z-50 flex w-64 flex-col border-r bg-background transition-transform duration-200 ease-in-out',
        isMobile
          ? isOpen
            ? 'translate-x-0'
            : '-translate-x-full'
          : 'translate-x-0',
        className
      )}
    >
      {/* Logo */}
      <div className="flex h-16 items-center border-b px-6">
        <Link href="/" className="flex items-center gap-2 font-semibold">
          <span className="text-xl">Hypesoft</span>
        </Link>
      </div>

      {/* Navigation */}
      <ScrollArea className="flex-1">
        <nav className="space-y-2 p-4">
          {sidebarItems.map((item) => renderSidebarItem(item))}
        </nav>
      </ScrollArea>

      {/* User Profile */}
      <div className="border-t p-4">
        <div className="flex items-center gap-3">
          <Avatar className="size-9">
            <AvatarImage src="/avatars/01.png" alt="User" />
            <AvatarFallback className="bg-primary text-primary-foreground">
              <FiUser className="size-4" />
            </AvatarFallback>
          </Avatar>
          <div className="flex-1 overflow-hidden">
            <p className="truncate text-sm font-medium">Admin User</p>
            <p className="truncate text-xs text-muted-foreground">admin@hypesoft.com</p>
          </div>
          <Button variant="ghost" size="icon" className="size-8">
            <FiLogOut className="size-4" />
          </Button>
        </div>
      </div>
    </aside>
  );
};
