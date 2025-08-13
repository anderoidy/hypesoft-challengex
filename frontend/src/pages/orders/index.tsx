import { FiSearch, FiPlus, FiFilter, FiDownload, FiMoreVertical, FiEdit2, FiTrash2, FiCheck, FiX, FiTruck, FiCreditCard } from 'react-icons/fi';
import { MainLayout } from '@/components/layout/MainLayout';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';

// Dados de exemplo para a tabela de pedidos
const orders = Array.from({ length: 10 }, (_, i) => ({
  id: `ORD-${String(i + 100).padStart(3, '0')}`,
  customer: `Cliente ${i + 1}`,
  date: new Date(2023, 9 - Math.floor(i / 3), 15 - (i % 15)).toISOString().split('T')[0],
  status: ['pending', 'processing', 'shipped', 'delivered', 'cancelled'][i % 5],
  payment: ['credit_card', 'debit_card', 'pix', 'bank_transfer', 'cash'][i % 5],
  total: Math.floor(Math.random() * 5000) + 100,
  items: Math.floor(Math.random() * 10) + 1,
}));

const statusStyles = {
  pending: { text: 'Pendente', class: 'bg-yellow-100 text-yellow-800' },
  processing: { text: 'Processando', class: 'bg-blue-100 text-blue-800' },
  shipped: { text: 'Enviado', class: 'bg-indigo-100 text-indigo-800' },
  delivered: { text: 'Entregue', class: 'bg-green-100 text-green-800' },
  cancelled: { text: 'Cancelado', class: 'bg-red-100 text-red-800' },
};

const paymentMethods = {
  credit_card: 'Cartão de Crédito',
  debit_card: 'Cartão de Débito',
  pix: 'PIX',
  bank_transfer: 'Transferência Bancária',
  cash: 'Dinheiro',
};

export default function OrdersPage() {
  return (
    <MainLayout>
      <div className="p-6">
        <div className="flex flex-col md:flex-row md:items-center md:justify-between mb-6">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Pedidos</h1>
            <p className="text-gray-500">Gerencie os pedidos dos seus clientes</p>
          </div>
          <Button className="mt-4 md:mt-0">
            <FiPlus className="mr-2 h-4 w-4" />
            Novo Pedido
          </Button>
        </div>

        <div className="bg-white rounded-lg shadow-sm border border-gray-200">
          <div className="p-4 border-b border-gray-200">
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
              <div className="relative flex-1">
                <FiSearch className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
                <Input
                  type="text"
                  placeholder="Buscar pedidos..."
                  className="pl-10 w-full"
                />
              </div>
              <div className="flex gap-2">
                <Button variant="outline">
                  <FiFilter className="mr-2 h-4 w-4" />
                  Filtrar
                </Button>
                <Button variant="outline">
                  <FiDownload className="mr-2 h-4 w-4" />
                  Exportar
                </Button>
              </div>
            </div>
          </div>

          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Nº do Pedido</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Cliente</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Data</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Total</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                  <th className="relative px-6 py-3"><span className="sr-only">Ações</span></th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {orders.map((order) => (
                  <tr key={order.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{order.id}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{order.customer}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {new Date(order.date).toLocaleDateString('pt-BR')}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(order.total)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center space-x-2">
                        <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${
                          statusStyles[order.status as keyof typeof statusStyles].class
                        }`}>
                          {statusStyles[order.status as keyof typeof statusStyles].text}
                        </span>
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button variant="ghost" size="sm" className="h-8 w-8 p-0">
                            <span className="sr-only">Abrir menu</span>
                            <FiMoreVertical className="h-4 w-4" />
                          </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end">
                          <DropdownMenuItem><FiEdit2 className="mr-2 h-4 w-4" />Editar</DropdownMenuItem>
                          <DropdownMenuItem className="text-red-600">
                            <FiTrash2 className="mr-2 h-4 w-4" />Excluir
                          </DropdownMenuItem>
                        </DropdownMenuContent>
                      </DropdownMenu>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </MainLayout>
  );
}
