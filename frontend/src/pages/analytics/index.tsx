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
  ArcElement,
} from 'chart.js';
import { FiDollarSign, FiShoppingBag, FiUsers, FiTrendingUp, FiPieChart } from 'react-icons/fi';
import { MainLayout } from '@/components/layout/MainLayout';
import { Bar, Line, Pie } from 'react-chartjs-2';

// Register ChartJS components
ChartJS.register(
  CategoryScale,
  LinearScale,
  BarElement,
  LineElement,
  PointElement,
  Title,
  Tooltip,
  Legend,
  ArcElement
);

// Sample data for the charts
const salesData = {
  labels: ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'],
  datasets: [
    {
      label: '2023',
      data: [6500, 5900, 8000, 8100, 5600, 5500, 7000, 7500, 8200, 8800, 9000, 9500],
      backgroundColor: 'rgba(99, 102, 241, 0.1)',
      borderColor: 'rgba(99, 102, 241, 1)',
      borderWidth: 2,
      tension: 0.3,
      fill: true,
    },
  ],
};

const revenueData = {
  labels: ['Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb', 'Dom'],
  datasets: [
    {
      label: 'Vendas',
      data: [12000, 19000, 15000, 25000, 22000, 30000, 28000],
      backgroundColor: 'rgba(16, 185, 129, 0.1)',
      borderColor: 'rgba(16, 185, 129, 1)',
      borderWidth: 2,
      tension: 0.3,
      fill: true,
    },
  ],
};

const categoryData = {
  labels: ['Eletrônicos', 'Roupas', 'Acessórios', 'Casa', 'Beleza'],
  datasets: [
    {
      data: [30000, 25000, 20000, 15000, 10000],
      backgroundColor: [
        'rgba(99, 102, 241, 0.8)',
        'rgba(16, 185, 129, 0.8)',
        'rgba(245, 158, 11, 0.8)',
        'rgba(239, 68, 68, 0.8)',
        'rgba(139, 92, 246, 0.8)',
      ],
      borderWidth: 0,
    },
  ],
};

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
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
        callback(value: any) {
          return 'R$ ' + value.toLocaleString('pt-BR');
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

const pieOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      position: 'right' as const,
    },
  },
};

const StatCard = ({ title, value, change, icon: Icon }: { title: string; value: string; change: string; icon: React.ElementType }) => (
  <div className="rounded-xl border border-gray-100 bg-white p-6 shadow-sm">
    <div className="flex items-center justify-between">
      <div>
        <p className="text-sm font-medium text-gray-500">{title}</p>
        <p className="mt-1 text-2xl font-semibold text-gray-900">{value}</p>
        <p className={`mt-1 text-sm ${parseFloat(change) >= 0 ? 'text-green-600' : 'text-red-600'}`}>
          {parseFloat(change) >= 0 ? '↑' : '↓'} {change} vs mês passado
        </p>
      </div>
      <div className="bg-indigo-50 text-indigo-600 rounded-lg p-3">
        <Icon size={24} />
      </div>
    </div>
  </div>
);

export default function AnalyticsPage() {
  return (
    <MainLayout>
      <div className="p-6">
        <div className="mb-6">
          <h1 className="text-2xl font-bold text-gray-900">Análises</h1>
          <p className="text-gray-500">Métricas e insights sobre o desempenho da sua loja</p>
        </div>

        {/* Stats Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-6">
          <StatCard
            title="Receita Total"
            value={
              new Intl.NumberFormat('pt-BR', {
                style: 'currency',
                currency: 'BRL',
              }).format(128000)
            }
            change="12.5%"
            icon={FiDollarSign}
          />
          <StatCard
            title="Vendas"
            value={new Intl.NumberFormat().format(1243)}
            change="8.2%"
            icon={FiShoppingBag}
          />
          <StatCard
            title="Clientes"
            value={new Intl.NumberFormat().format(842)}
            change="5.7%"
            icon={FiUsers}
          />
          <StatCard
            title="Crescimento"
            value="+18.3%"
            change="2.4%"
            icon={FiTrendingUp}
          />
        </div>

        {/* Charts */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-6">
          <div className="bg-white p-6 rounded-xl border border-gray-100 shadow-sm">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-semibold text-gray-900">Receita Mensal</h2>
              <div className="bg-indigo-50 text-indigo-600 rounded-lg p-2">
                <FiDollarSign size={18} />
              </div>
            </div>
            <div className="h-80">
              <Line data={salesData} options={chartOptions} />
            </div>
          </div>

          <div className="bg-white p-6 rounded-xl border border-gray-100 shadow-sm">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-semibold text-gray-900">Vendas por Categoria</h2>
              <div className="bg-indigo-50 text-indigo-600 rounded-lg p-2">
                <FiPieChart size={18} />
              </div>
            </div>
            <div className="h-80">
              <Pie data={categoryData} options={pieOptions} />
            </div>
          </div>
        </div>

        <div className="bg-white p-6 rounded-xl border border-gray-100 shadow-sm">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-lg font-semibold text-gray-900">Vendas da Semana</h2>
            <select className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500">
              <option>Últimos 7 dias</option>
              <option>Últimos 30 dias</option>
              <option>Últimos 90 dias</option>
            </select>
          </div>
          <div className="h-80">
            <Bar data={revenueData} options={chartOptions} />
          </div>
        </div>
      </div>
    </MainLayout>
  );
}
