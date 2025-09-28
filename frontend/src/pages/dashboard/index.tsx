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
          {parseFloat(change) >= 0 ? '↑' : '↓'} {change} vs last month
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
  const [productCount, setProductCount] = useState<number>(0);
  const [totalValue, setTotalValue] = useState<number>(0);
  const [loading, setLoading] = useState<boolean>(false);
  const [debugInfo, setDebugInfo] = useState<string>('Inicializando...');
  const [error, setError] = useState<string | null>(null);

  // Função para buscar produtos COM LOGS MELHORADOS
  useEffect(() => {
    let isMounted = true; // Evitar race conditions

    const fetchProducts = async (): Promise<void> => {
      try {
        setLoading(true);
        setError(null);
        setDebugInfo('🔍 Verificando token...');

        // Verificar se estamos no browser
        if (typeof window === 'undefined') {
          setDebugInfo('❌ Não está no browser');
          return;
        }

        const token = localStorage.getItem('accessToken');
        console.log('🔐 Token check:', {
          exists: !!token,
          length: token?.length,
          firstChars: token?.substring(0, 20) + '...'
        });

        if (!token) {
          setDebugInfo('❌ Token não encontrado no localStorage');
          return;
        }

        setDebugInfo('✅ Token encontrado - fazendo requisição...');
        
        // Construir URL da API
        const apiUrl: string = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5010';
        const endpoint = '/api/Products';
        const fullUrl = `${apiUrl}${endpoint}?page=1&pageSize=1000`;
        
        console.log('🌐 Fazendo requisição para:', fullUrl);
        console.log('📋 Headers:', {
          'Authorization': `Bearer ${token.substring(0, 20)}...`,
          'Content-Type': 'application/json'
        });

        setDebugInfo(`🌐 Chamando: ${fullUrl}`);

        const response = await fetch(fullUrl, {
          method: 'GET',
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json',
            'Accept': 'application/json'
          },
        });

        console.log('📡 Response:', {
          status: response.status,
          statusText: response.statusText,
          ok: response.ok,
          headers: Object.fromEntries(response.headers.entries())
        });

        if (!isMounted) return; // Component foi desmontado

        if (!response.ok) {
          const errorText = await response.text();
          console.error('❌ Erro HTTP:', {
            status: response.status,
            statusText: response.statusText,
            body: errorText
          });
          
          setError(`HTTP ${response.status}: ${response.statusText}`);
          setDebugInfo(`❌ Erro ${response.status}: ${errorText}`);
          return;
        }

        const data: ApiResponse = await response.json();
        console.log('📦 Resposta completa da API:', data);
        console.log('🏷️ Estrutura dos dados:', {
          hasItems: 'items' in data,
          hasData: 'data' in data,
          hasValue: 'value' in data,
          isSuccess: data.isSuccess,
          hasDataFlag: data.hasData,
          isArray: Array.isArray(data),
          keys: Object.keys(data),
          type: typeof data,
          valueType: typeof data.value,
          valueKeys: data.value ? Object.keys(data.value) : 'N/A'
        });

        // Verificar diferentes estruturas possíveis da sua API
        let products: Product[] = [];
        
        // Estrutura da sua API: { value: {...}, isSuccess: true, hasData: boolean }
        if (data.isSuccess && data.value) {
          console.log('✅ API Success - Verificando value:', data.value);
          
          if (typeof data.value === 'object' && !Array.isArray(data.value)) {
            if (data.value.items && Array.isArray(data.value.items)) {
              products = data.value.items;
              console.log('📦 Produtos encontrados em value.items:', products.length);
            } else if (data.value.data && Array.isArray(data.value.data)) {
              products = data.value.data;
              console.log('📦 Produtos encontrados em value.data:', products.length);
            } else if (data.value.products && Array.isArray(data.value.products)) {
              products = data.value.products;
              console.log('📦 Produtos encontrados em value.products:', products.length);
            } else {
              console.log('🔍 Estrutura do value:', data.value);
              console.log('🔍 Chaves do value:', Object.keys(data.value || {}));
            }
          } else if (Array.isArray(data.value)) {
            products = data.value;
            console.log('📦 Produtos encontrados em value (array):', products.length);
          }
        } 
        // Fallback para outras estruturas
        else if (data.items && Array.isArray(data.items)) {
          products = data.items;
        } else if (data.data && Array.isArray(data.data)) {
          products = data.data;
        } else if (Array.isArray(data)) {
          products = data as Product[];
        } else {
          console.warn('⚠️ Estrutura de dados não reconhecida:', {
            isSuccess: data.isSuccess,
            hasData: data.hasData,
            valueExists: !!data.value,
            dataStructure: data
          });
        }

        console.log('📦 Produtos extraídos:', products);
        console.log('🔢 Quantidade de produtos:', products.length);

        if (!isMounted) return;

        setProductCount(products.length);
        
        // Mensagem mais específica baseada no resultado
        if (products.length === 0) {
          if (data.isSuccess) {
            setDebugInfo(`✅ Conectado à API - Nenhum produto cadastrado ainda`);
          } else {
            setDebugInfo(`❌ API retornou erro: ${data.successMessage || 'Erro desconhecido'}`);
          }
        } else {
          setDebugInfo(`✅ ${products.length} produtos encontrados`);
        }
        
        // Calcular valor total do estoque
        if (products.length > 0) {
          const total = products.reduce((sum: number, product: Product) => {
            // Limpar e converter price
            let priceValue: string | number = product.price;
            let price = 0;
            
            if (typeof priceValue === 'string') {
              // Se for string, remove R$ e espaços
              let cleanPrice = priceValue
                .replace(/R\$\s?/, '') // Remove R$ 
                .trim();
              price = parseFloat(cleanPrice) || 0;
            } else if (typeof priceValue === 'number') {
              // Se já for número, usa direto
              price = priceValue;
            }
            
            const stock = parseInt(String(product.stockQuantity || product.stock || 0));
            const value = price * stock;
            
            console.log('💰 Produto - Debug detalhado:', {
              name: product.name,
              rawPrice: product.price,
              typeOfPrice: typeof product.price,
              finalPrice: price,
              stock: stock,
              calculation: `${price} × ${stock} = ${value}`,
              value: `R$ ${value.toFixed(2)}`
            });
            
            return sum + value;
          }, 0);
          
          setTotalValue(total);
          console.log('💰 CÁLCULO FINAL:', {
            totalBruto: total,
            totalFormatado: formatCurrency(total),
            quantidadeProdutos: products.length
          });
        }

      } catch (error) {
        console.error('🚨 Erro na requisição completo:', error);
        if (!isMounted) return;
        
        const errorMessage = error instanceof Error ? error.message : 'Erro desconhecido';
        setError(errorMessage);
        setDebugInfo(`🚨 Erro: ${errorMessage}`);
      } finally {
        if (isMounted) {
          setLoading(false);
        }
      }
    };

    fetchProducts();

    // Cleanup function
    return () => {
      isMounted = false;
    };
  }, []);

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
            {productCount > 0 && <span className="ml-2 text-green-600 dark:text-green-400">({productCount} produtos cadastrados)</span>}
          </p>
        </div>

        {/* Stats Grid - COM DADOS REAIS DOS PRODUTOS */}
        <div className="mb-8 grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-4">
          <StatCard 
            title="Total Revenue" 
            value="$24,780" 
            change="+12.5%" 
            icon={FiDollarSign} 
          />
          <StatCard 
            title="Total Orders" 
            value="1,245" 
            change="+8.3%" 
            icon={FiShoppingBag} 
          />
          <StatCard 
            title="New Customers" 
            value="342" 
            change="+5.2%" 
            icon={FiUsers} 
          />
          {/* ✨ CARD COM DADOS REAIS DOS PRODUTOS */}
          <StatCard 
            title="Produtos Cadastrados" 
            value={loading ? "Carregando..." : productCount.toString()}
            change="+15.8%" 
            icon={FiPackage}
            isReal={true}
            loading={loading}
          />
        </div>

        {/* Seção de Valor do Estoque - SE TIVER PRODUTOS */}
        {totalValue > 0 && (
          <div className="mb-8">
            <div className="rounded-xl border border-blue-200 dark:border-blue-800 bg-blue-50 dark:bg-blue-900/20 p-6">
              <div className="flex items-center justify-between">
                <div>
                  <h3 className="text-lg font-semibold text-blue-900 dark:text-blue-100">
                    💼 Valor Total do Estoque
                  </h3>
                  <p className="text-3xl font-bold text-blue-700 dark:text-blue-300 mt-2">
                    {formatCurrency(totalValue)}
                  </p>
                  <p className="text-blue-600 dark:text-blue-400 text-sm mt-1">
                    Baseado em {productCount} produtos cadastrados
                  </p>
                </div>
                <div className="p-4 bg-blue-100 dark:bg-blue-800 rounded-lg">
                  <FiDollarSign size={32} className="text-blue-600 dark:text-blue-400" />
                </div>
              </div>
              <div className="mt-4">
                <button 
                  onClick={() => window.location.href = '/products'}
                  className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
                >
                  Ver Produtos
                </button>
              </div>
            </div>
          </div>
        )}

        {/* Charts */}
        <div className="mb-8 grid grid-cols-1 gap-6 lg:grid-cols-2">
          <div className="rounded-xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-6 shadow-sm">
            <div className="mb-6 flex items-center justify-between">
              <h2 className="text-lg font-semibold text-gray-900 dark:text-white">Revenue Overview</h2>
              <select className="focus:ring-primary-500 rounded-md border-0 bg-gray-100 dark:bg-gray-800 text-sm focus:ring-2 text-gray-900 dark:text-white">
                <option>Last 7 days</option>
                <option>Last 30 days</option>
                <option>Last 3 months</option>
              </select>
            </div>
            <div className="h-80">
              <Line data={revenueData} options={chartOptions} />
            </div>
          </div>

          <div className="rounded-xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-6 shadow-sm">
            <div className="mb-6 flex items-center justify-between">
              <h2 className="text-lg font-semibold text-gray-900 dark:text-white">Sales Overview</h2>
              <select className="focus:ring-primary-500 rounded-md border-0 bg-gray-100 dark:bg-gray-800 text-sm focus:ring-2 text-gray-900 dark:text-white">
                <option>Last 7 days</option>
                <option>Last 30 days</option>
                <option>Last 3 months</option>
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
            <h2 className="text-lg font-semibold text-gray-900 dark:text-white">Recent Orders</h2>
            <button className="text-primary-600 dark:text-primary-400 hover:text-primary-700 dark:hover:text-primary-300 text-sm font-medium">
              View All
            </button>
          </div>
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
              <thead>
                <tr>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 dark:text-gray-400">
                    Order ID
                  </th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 dark:text-gray-400">
                    Customer
                  </th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 dark:text-gray-400">
                    Date
                  </th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 dark:text-gray-400">
                    Amount
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