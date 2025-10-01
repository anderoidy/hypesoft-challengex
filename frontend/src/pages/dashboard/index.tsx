'use client'; 
import {
  BarElement,
  CategoryScale,
  Chart as ChartJS,
  Legend,
  LinearScale,
  LineElement,
  PointElement,
  Title,
  Tooltip,
} from 'chart.js';
import React, { useState, useEffect } from 'react';
import { Bar, Line } from 'react-chartjs-2';
import { FiDollarSign, FiShoppingBag, FiTrendingUp, FiUsers, FiPackage } from 'react-icons/fi';

import { MainLayout } from '@/components/layout/MainLayout';
import { ProtectedRoute } from '@/components/ProtectedRoute';
import { useDashboardMetrics } from '@/hooks/useDashboardMetrics';

// Register ChartJS components
ChartJS.register(
  CategoryScale,
  LinearScale,
  BarElement,
  LineElement,
  PointElement,
  Title,
  Tooltip,
  Legend
);

// Definindo interfaces para tipagem
interface Product {
  id?: string | number;
  name: string;
  price: string | number;
  stockQuantity?: number;
  stock?: number;
}

interface ApiResponse {
  value?: {
    items?: Product[];
    data?: Product[];
    products?: Product[];
  } | Product[];
  items?: Product[];
  data?: Product[];
  isSuccess?: boolean;
  hasData?: boolean;
  successMessage?: string;
}

interface StatCardProps {
  title: string;
  value: string;
  change: string;
  icon: React.ComponentType<{ size: number }>;
  isReal?: boolean;
  loading?: boolean;
}

interface Order {
  id: string;
  customer: string;
  date: string;
  amount: string;
  status: string;
}

// Sample data for the charts
const salesData = {
  labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul'],
  datasets: [
    {
      label: '2023',
      data: [65, 59, 80, 81, 56, 55, 40],
      backgroundColor: 'rgba(14, 165, 233, 0.1)',
      borderColor: 'rgba(14, 165, 233, 1)',
      borderWidth: 2,
      tension: 0.3,
      fill: true,
    },
  ],
};

const revenueData = {
  labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul'],
  datasets: [
    {
      label: 'Revenue',
      data: [12000, 19000, 15000, 25000, 22000, 30000, 28000],
      backgroundColor: 'rgba(139, 92, 246, 0.1)',
      borderColor: 'rgba(139, 92, 246, 1)',
      borderWidth: 2,
      tension: 0.3,
      fill: true,
    },
  ],
};

const chartOptions = {
  responsive: true,
  plugins: {
    legend: {
      display: false,
    },
  },
  scales: {
    y: {
      beginAtZero: true,
      grid: {
        display: true,
        drawBorder: false,
      },
      ticks: {
        callback(value: string | number): string {
          const numValue = typeof value === 'string' ? parseFloat(value) : value;
          return '$' + numValue.toLocaleString();
        },
      },
    },
    x: {
      grid: {
        display: false,
      },
    },
  },
};

const StatCard: React.FC<StatCardProps> = ({ 
  title, 
  value, 
  change, 
  icon: Icon, 
  isReal = false,
  loading = false 
}) => (
  <div className="rounded-xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-6 shadow-sm relative">
    {isReal && (
      <div className="absolute top-2 right-2 text-xs bg-green-100 dark:bg-green-900 text-green-800 dark:text-green-200 px-2 py-1 rounded-full">
        Real
      </div>
    )}
    <div className="flex items-center justify-between">
      <div>
        <p className="text-sm font-medium text-gray-500 dark:text-gray-400">{title}</p>
        {loading ? (
          <div className="mt-1 h-8 w-16 bg-gray-200 dark:bg-gray-700 rounded animate-pulse"></div>
        ) : (
          <p className="mt-1 text-2xl font-semibold text-gray-900 dark:text-white">{value}</p>
        )}
        <p className={`mt-1 text-sm ${parseFloat(change) >= 0 ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'}`}>
          {parseFloat(change) >= 0 ? '↑' : '↓'} {change} em comparação com o mês passado
        </p>
      </div>
      <div className="bg-primary-50 dark:bg-primary-900 text-primary-600 dark:text-primary-400 rounded-lg p-3">
        <Icon size={24} />
      </div>
    </div>
  </div>
);

const DashboardPage: React.FC = () => {
  // Estados para buscar produtos reais
// Usar hook de métricas do dashboard
const { totalProducts, totalValue, loading, error, refresh } = useDashboardMetrics();


  // Função para buscar produtos COM LOGS MELHORADOS
  

  // Formatação brasileira
  const formatCurrency = (value: number): string => {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(value);
  };

  // Função para recarregar os dados
  const handleReload = (): void => {
    window.location.reload();
  };

  // Dados das ordens mockados com tipagem
  const orders: Order[] = [
    { id: '#ORD-001', customer: 'John Smith', date: '2023-05-15', amount: '$125.00', status: 'Completed' },
    { id: '#ORD-002', customer: 'Sarah Johnson', date: '2023-05-14', amount: '$89.99', status: 'Processing' },
    { id: '#ORD-003', customer: 'Michael Brown', date: '2023-05-14', amount: '$234.50', status: 'Shipped' },
    { id: '#ORD-004', customer: 'Emily Davis', date: '2023-05-13', amount: '$67.25', status: 'Completed' },
    { id: '#ORD-005', customer: 'Robert Wilson', date: '2023-05-12', amount: '$199.99', status: 'Pending' },
  ];

  return (
    <ProtectedRoute>
      <MainLayout title="Dashboard">
        <div className="mb-6">
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Dashboard</h1>
          <p className="text-gray-500 dark:text-gray-400">          
            {totalProducts> 0 && <span className="ml-2 text-green-600 dark:text-green-400">({totalProducts} produtos cadastrados)</span>}
          </p>
        </div>

        {/* Stats Grid - COM DADOS REAIS DOS PRODUTOS */}
        <div className="mb-8 grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-4">
          <StatCard 
            title="Total de Receita" 
            value={formatCurrency(totalValue)} 
            change="+12.5%" 
            icon={FiDollarSign} 
          />
          <StatCard 
            title="Total de Pedidos" 
            value="1,245" 
            change="+8.3%" 
            icon={FiShoppingBag} 
          />
          <StatCard 
            title="Novos Clientes" 
            value="342" 
            change="+5.2%" 
            icon={FiUsers} 
          />
          {/* ✨ CARD COM DADOS REAIS DOS PRODUTOS */}
          <StatCard 
            title="Produtos Cadastrados" 
            value={loading ? "Carregando..." : totalProducts.toString()}
            change="+15.8%" 
            icon={FiPackage}
            isReal={true}
            loading={loading}
          />
        </div>

        {/* Charts */}
        <div className="mb-8 grid grid-cols-1 gap-6 lg:grid-cols-2">
          <div className="rounded-xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-6 shadow-sm">
            <div className="mb-6 flex items-center justify-between">
              <h2 className="text-lg font-semibold text-gray-900 dark:text-white">Visão Geral da Receita</h2>
              <select className="focus:ring-primary-500 rounded-md border-0 bg-gray-100 dark:bg-gray-800 text-sm focus:ring-2 text-gray-900 dark:text-white">
                <option>Ultimos 7 dias</option>
                <option>Ultimos 30 dias</option>
                <option>Ultimos 3 meses</option>
              </select>
            </div>
            <div className="h-80">
              <Line data={revenueData} options={chartOptions} />
            </div>
          </div>

          <div className="rounded-xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-6 shadow-sm">
            <div className="mb-6 flex items-center justify-between">
              <h2 className="text-lg font-semibold text-gray-900 dark:text-white">Visão Geral de Vendas</h2>
              <select className="focus:ring-primary-500 rounded-md border-0 bg-gray-100 dark:bg-gray-800 text-sm focus:ring-2 text-gray-900 dark:text-white">
                <option>Ultimos 7 dias</option>
                <option>Ultimos 30 dias</option>
                <option>Ultimos 3 meses</option>
              </select>
            </div>
            <div className="h-80">
              <Bar data={salesData} options={chartOptions} />
            </div>
          </div>
        </div>

        {/* Recent Orders */}
        <div className="rounded-xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-6 shadow-sm">
          <div className="mb-6 flex items-center justify-between">
            <h2 className="text-lg font-semibold text-gray-900 dark:text-white">Pedidos Recentes</h2>
            <button className="text-primary-600 dark:text-primary-400 hover:text-primary-700 dark:hover:text-primary-300 text-sm font-medium">
              Ver tudo
            </button>
          </div>
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
              <thead>
                <tr>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 dark:text-gray-400">
                    ID do Pedido
                  </th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 dark:text-gray-400">
                    Cliente
                  </th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 dark:text-gray-400">
                    Data
                  </th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 dark:text-gray-400">
                    Quantidade  
                  </th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 dark:text-gray-400">
                    Status
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200 dark:divide-gray-700 bg-white dark:bg-gray-900">
                {orders.map((order) => (
                  <tr key={order.id} className="hover:bg-gray-50 dark:hover:bg-gray-800">
                    <td className="whitespace-nowrap px-6 py-4 text-sm font-medium text-gray-900 dark:text-white">
                      {order.id}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500 dark:text-gray-400">
                      {order.customer}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500 dark:text-gray-400">
                      {order.date}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500 dark:text-gray-400">
                      {order.amount}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4">
                      <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${
                        order.status === 'Completed' 
                          ? 'bg-green-100 dark:bg-green-900 text-green-800 dark:text-green-200' 
                          : order.status === 'Processing' 
                          ? 'bg-yellow-100 dark:bg-yellow-900 text-yellow-800 dark:text-yellow-200'
                          : order.status === 'Shipped'
                          ? 'bg-blue-100 dark:bg-blue-900 text-blue-800 dark:text-blue-200'
                          : 'bg-gray-100 dark:bg-gray-700 text-gray-800 dark:text-gray-200'
                      }`}>
                        {order.status}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </MainLayout>
    </ProtectedRoute> 
  );
};

export default DashboardPage;