import { useState, useEffect } from 'react';

interface Product {
  id: string;
  name: string;
  price: number;
  stockQuantity: number;
  description?: string;
  sku?: string;
}

interface DashboardMetrics {
  totalRevenue: number;
  totalProducts: number;
  lowStockProducts: number;
  averagePrice: number;
  totalValue: number;
  loading: boolean;
  error: string | null;
}

export const useDashboardMetrics = () => {
  const [metrics, setMetrics] = useState<DashboardMetrics>({
    totalRevenue: 0,
    totalProducts: 0,
    lowStockProducts: 0,
    averagePrice: 0,
    totalValue: 0,
    loading: true,
    error: null,
  });

  const fetchProducts = async () => {
    try {
      setMetrics(prev => ({ ...prev, loading: true, error: null }));

      // ✅ DEBUG - Log dos valores
      const token = localStorage.getItem('token');
      const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';
      
      console.log('🔍 Debug Dashboard:');
      console.log('Token existe:', !!token);
      console.log('API URL:', apiUrl);
      console.log('URL completa:', `${apiUrl}/api/Products?page=1&pageSize=100`);

      if (!token) {
        throw new Error('Token de autenticação não encontrado. Faça login novamente.');
      }

      const response = await fetch(`${apiUrl}/api/Products?page=1&pageSize=100`, {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
      });

      console.log('📊 Response status:', response.status);
      console.log('📊 Response ok:', response.ok);

      if (!response.ok) {
        const errorText = await response.text();
        console.error('❌ Error response:', errorText);
        
        if (response.status === 401) {
          throw new Error('Sessão expirada. Faça login novamente.');
        } else if (response.status === 500) {
          throw new Error('Erro interno do servidor. Tente novamente em alguns minutos.');
        } else {
          throw new Error(`Erro ${response.status}: ${errorText}`);
        }
      }

      const data = await response.json();
      console.log('📊 Data received:', data);
      
      // ✅ VERIFICAR ESTRUTURA DOS DADOS
      const products: Product[] = data.items || data || [];
      console.log('📦 Produtos encontrados:', products.length);

      // ✅ CÁLCULOS
      const totalProducts = products.length;
      const totalValue = products.reduce((sum, product) => {
        const price = Number(product.price) || 0;
        const stock = Number(product.stockQuantity) || 0;
        return sum + (price * stock);
      }, 0);
      
      const totalRevenue = totalValue * 0.7;
      const lowStockProducts = products.filter(p => (Number(p.stockQuantity) || 0) < 10).length;
      const averagePrice = totalProducts > 0 
        ? products.reduce((sum, p) => sum + (Number(p.price) || 0), 0) / totalProducts 
        : 0;

      console.log('📊 Métricas calculadas:', {
        totalProducts,
        totalValue,
        totalRevenue,
        lowStockProducts,
        averagePrice
      });

      setMetrics({
        totalRevenue,
        totalProducts,
        lowStockProducts,
        averagePrice,
        totalValue,
        loading: false,
        error: null,
      });

    } catch (error: any) {
      console.error('❌ Erro completo:', error);
      setMetrics(prev => ({
        ...prev,
        loading: false,
        error: error?.message || 'Erro ao carregar métricas do dashboard'
      }));
    }
  };

  useEffect(() => {
    fetchProducts();
  }, []);

  return {
    totalRevenue: metrics.totalRevenue,
    totalProducts: metrics.totalProducts,
    lowStockProducts: metrics.lowStockProducts,
    averagePrice: metrics.averagePrice,
    totalValue: metrics.totalValue,
    loading: metrics.loading,
    error: metrics.error,
    refresh: fetchProducts
  };
};
