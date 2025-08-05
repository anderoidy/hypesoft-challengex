import React, { ReactNode, useState } from 'react';
import Head from 'next/head';
import Link from 'next/link';
import { useRouter } from 'next/router';
import { FiHome, FiShoppingBag, FiUsers, FiSettings, FiMenu, FiX, FiChevronDown, FiSearch, FiBell, FiUser } from 'react-icons/fi';
import { theme } from '@/lib/theme';

type SidebarItem = {
  name: string;
  icon: React.ReactNode;
  path: string;
  children?: SidebarItem[];
};

const sidebarItems: SidebarItem[] = [
  { name: 'Dashboard', icon: <FiHome size={20} />, path: '/dashboard' },
  { 
    name: 'Products', 
    icon: <FiShoppingBag size={20} />, 
    path: '/products',
    children: [
      { name: 'All Products', path: '/products' },
      { name: 'Categories', path: '/products/categories' },
      { name: 'Inventory', path: '/products/inventory' },
    ]
  },
  { name: 'Customers', icon: <FiUsers size={20} />, path: '/customers' },
  { name: 'Settings', icon: <FiSettings size={20} />, path: '/settings' },
];

interface MainLayoutProps {
  children: ReactNode;
  title?: string;
}

const MainLayout: React.FC<MainLayoutProps> = ({ children, title = 'Hypesoft Dashboard' }) => {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [expandedItems, setExpandedItems] = useState<Record<string, boolean>>({});
  const router = useRouter();

  const toggleSidebar = () => {
    setSidebarOpen(!sidebarOpen);
  };

  const toggleItem = (name: string) => {
    setExpandedItems(prev => ({
      ...prev,
      [name]: !prev[name]
    }));
  };

  const isActive = (path: string) => {
    return router.pathname === path || router.pathname.startsWith(`${path}/`);
  };

  const renderSidebarItem = (item: SidebarItem, depth = 0) => {
    const hasChildren = item.children && item.children.length > 0;
    const isItemActive = isActive(item.path);
    const isExpanded = expandedItems[item.name];

    return (
      <div key={item.path} className="mb-1">
        <div
          onClick={() => hasChildren ? toggleItem(item.name) : router.push(item.path)}
          className={`flex items-center justify-between px-4 py-3 rounded-lg cursor-pointer transition-colors ${
            isItemActive 
              ? 'bg-primary-100 text-primary-700 font-medium' 
              : 'text-gray-600 hover:bg-gray-100'
          }`}
          style={{ paddingLeft: `${depth * 16 + 16}px` }}
        >
          <div className="flex items-center">
            <span className="mr-3">{item.icon}</span>
            <span>{item.name}</span>
          </div>
          {hasChildren && (
            <FiChevronDown 
              className={`transition-transform ${isExpanded ? 'transform rotate-180' : ''}`} 
              size={18} 
            />
          )}
        </div>
        {hasChildren && isExpanded && (
          <div className="mt-1">
            {item.children?.map(child => renderSidebarItem(child, depth + 1))}
          </div>
        )}
      </div>
    );
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <Head>
        <title>{title}</title>
        <meta name="description" content="Hypesoft Challenge - Product Management System" />
        <link rel="icon" href="/favicon.ico" />
      </Head>

      {/* Mobile Sidebar Overlay */}
      {sidebarOpen && (
        <div 
          className="fixed inset-0 z-40 bg-black bg-opacity-50 lg:hidden"
          onClick={toggleSidebar}
        />
      )}

      {/* Sidebar */}
      <aside 
        className={`fixed inset-y-0 left-0 z-50 w-64 bg-white shadow-lg transform ${
          sidebarOpen ? 'translate-x-0' : '-translate-x-full'
        } lg:translate-x-0 transition-transform duration-200 ease-in-out`}
      >
        <div className="flex flex-col h-full">
          {/* Logo */}
          <div className="flex items-center justify-between h-16 px-6 border-b border-gray-200">
            <Link href="/" className="text-xl font-bold text-primary-600">
              Hypesoft
            </Link>
            <button 
              onClick={toggleSidebar}
              className="p-1 rounded-md lg:hidden text-gray-500 hover:text-gray-700"
            >
              <FiX size={24} />
            </button>
          </div>

          {/* Navigation */}
          <nav className="flex-1 px-4 py-6 overflow-y-auto">
            {sidebarItems.map(item => renderSidebarItem(item))}
          </nav>

          {/* User Profile */}
          <div className="p-4 border-t border-gray-200">
            <div className="flex items-center">
              <div className="w-10 h-10 rounded-full bg-primary-100 flex items-center justify-center text-primary-600">
                <FiUser size={20} />
              </div>
              <div className="ml-3">
                <p className="text-sm font-medium text-gray-900">Admin User</p>
                <p className="text-xs text-gray-500">admin@hypesoft.com</p>
              </div>
            </div>
          </div>
        </div>
      </aside>

      {/* Main Content */}
      <div className="lg:pl-64">
        {/* Header */}
        <header className="sticky top-0 z-30 flex items-center justify-between h-16 px-6 bg-white border-b border-gray-200">
          <div className="flex items-center">
            <button 
              onClick={toggleSidebar}
              className="p-1 mr-2 text-gray-500 rounded-md lg:hidden hover:text-gray-600"
            >
              <FiMenu size={24} />
            </button>
            
            {/* Search Bar */}
            <div className="relative max-w-md lg:ml-4">
              <div className="absolute inset-y-0 left-0 flex items-center pl-3 pointer-events-none">
                <FiSearch className="text-gray-400" />
              </div>
              <input
                type="text"
                placeholder="Search..."
                className="w-full py-2 pl-10 pr-4 text-sm bg-gray-100 border-0 rounded-lg focus:ring-2 focus:ring-primary-500 focus:bg-white focus:outline-none"
              />
            </div>
          </div>

          <div className="flex items-center space-x-4">
            <button className="p-1 text-gray-500 rounded-full hover:bg-gray-100">
              <FiBell size={20} />
            </button>
            <div className="w-px h-6 bg-gray-200"></div>
            <div className="flex items-center">
              <div className="w-8 h-8 rounded-full bg-primary-100 flex items-center justify-center text-primary-600">
                <FiUser size={16} />
              </div>
              <span className="ml-2 text-sm font-medium text-gray-700 hidden md:inline">
                Admin User
              </span>
            </div>
          </div>
        </header>

        {/* Page Content */}
        <main className="p-6">
          {children}
        </main>
      </div>
    </div>
  );
};

export default MainLayout;
