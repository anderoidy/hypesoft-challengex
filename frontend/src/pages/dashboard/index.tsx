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
import React from 'react';
import { Bar, Line } from 'react-chartjs-2';
import { FiDollarSign, FiShoppingBag, FiTrendingUp,FiUsers } from 'react-icons/fi';

import { MainLayout } from '@/components/layout/MainLayout';

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
        callback(value: any) {
          return '$' + value.toLocaleString();
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

const StatCard = ({ title, value, change, icon: Icon }: { title: string; value: string; change: string; icon: React.ElementType }) => (
  <div className="rounded-xl border border-gray-100 bg-white p-6 shadow-sm">
    <div className="flex items-center justify-between">
      <div>
        <p className="text-sm font-medium text-gray-500">{title}</p>
        <p className="mt-1 text-2xl font-semibold text-gray-900">{value}</p>
        <p className={`mt-1 text-sm ${parseFloat(change) >= 0 ? 'text-green-600' : 'text-red-600'}`}>
          {parseFloat(change) >= 0 ? '↑' : '↓'} {change} vs last month
        </p>
      </div>
      <div className="bg-primary-50 text-primary-600 rounded-lg p-3">
        <Icon size={24} />
      </div>
    </div>
  </div>
);

const DashboardPage = () => {
  return (
    <MainLayout title="Dashboard">
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
        <p className="text-gray-500">Welcome back! Here's what's happening with your store today.</p>
      </div>

      {/* Stats Grid */}
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
        <StatCard 
          title="Conversion Rate" 
          value="3.42%" 
          change="+0.8%" 
          icon={FiTrendingUp} 
        />
      </div>

      {/* Charts */}
      <div className="mb-8 grid grid-cols-1 gap-6 lg:grid-cols-2">
        <div className="rounded-xl border border-gray-100 bg-white p-6 shadow-sm">
          <div className="mb-6 flex items-center justify-between">
            <h2 className="text-lg font-semibold text-gray-900">Revenue Overview</h2>
            <select className="focus:ring-primary-500 rounded-md border-0 bg-gray-100 text-sm focus:ring-2">
              <option>Last 7 days</option>
              <option>Last 30 days</option>
              <option>Last 3 months</option>
            </select>
          </div>
          <div className="h-80">
            <Line data={revenueData} options={chartOptions} />
          </div>
        </div>

        <div className="rounded-xl border border-gray-100 bg-white p-6 shadow-sm">
          <div className="mb-6 flex items-center justify-between">
            <h2 className="text-lg font-semibold text-gray-900">Sales Overview</h2>
            <select className="focus:ring-primary-500 rounded-md border-0 bg-gray-100 text-sm focus:ring-2">
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
      <div className="rounded-xl border border-gray-100 bg-white p-6 shadow-sm">
        <div className="mb-6 flex items-center justify-between">
          <h2 className="text-lg font-semibold text-gray-900">Recent Orders</h2>
          <button className="text-primary-600 hover:text-primary-700 text-sm font-medium">
            View All
          </button>
        </div>
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead>
              <tr>
                <th scope="col" className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Order ID
                </th>
                <th scope="col" className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Customer
                </th>
                <th scope="col" className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Date
                </th>
                <th scope="col" className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Amount
                </th>
                <th scope="col" className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Status
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 bg-white">
              {[
                { id: '#ORD-001', customer: 'John Smith', date: '2023-05-15', amount: '$125.00', status: 'Completed' },
                { id: '#ORD-002', customer: 'Sarah Johnson', date: '2023-05-14', amount: '$89.99', status: 'Processing' },
                { id: '#ORD-003', customer: 'Michael Brown', date: '2023-05-14', amount: '$234.50', status: 'Shipped' },
                { id: '#ORD-004', customer: 'Emily Davis', date: '2023-05-13', amount: '$67.25', status: 'Completed' },
                { id: '#ORD-005', customer: 'Robert Wilson', date: '2023-05-12', amount: '$199.99', status: 'Pending' },
              ].map((order) => (
                <tr key={order.id} className="hover:bg-gray-50">
                  <td className="whitespace-nowrap px-6 py-4 text-sm font-medium text-gray-900">
                    {order.id}
                  </td>
                  <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                    {order.customer}
                  </td>
                  <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                    {order.date}
                  </td>
                  <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                    {order.amount}
                  </td>
                  <td className="whitespace-nowrap px-6 py-4">
                    <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${
                      order.status === 'Completed' 
                        ? 'bg-green-100 text-green-800' 
                        : order.status === 'Processing' 
                        ? 'bg-yellow-100 text-yellow-800'
                        : order.status === 'Shipped'
                        ? 'bg-blue-100 text-blue-800'
                        : 'bg-gray-100 text-gray-800'
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
  );
};

export default DashboardPage;
