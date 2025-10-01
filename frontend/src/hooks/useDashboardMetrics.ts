import { useState, useEffect } from 'react';

interface Product {
  id: string;
  name: string;
  price: number | string;
  stockQuantity: number | string;
  description?: string;
  sku?: string;
  stock?: number | string;
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
      const token = localStorage.getItem('accessToken');
      const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:80';
      
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
      
      // ✅ VERIFICAR ESTRUTURA DOS DADOS - MELHORADO
      const products: Product[] = data.items || data.value?.items || data.value?.data || data.value?.products || data.items || data.data || data.products || [];
      console.log('📦 Produtos encontrados:', products.length);

      // ✅ CÁLCULOS CORRIGIDOS
      const totalProducts = products.length;
      const totalValue = products.reduce((sum, product) => {
        // Converter price para número, tratando tanto string quanto number
        let price = 0;
        if (typeof product.price === 'string') {
          // Remove símbolos de moeda e espaços, trata formatação brasileira
          let cleanPrice = product.price
            .replace(/R\$\s?/g, '') // Remove R$ 
            .replace(/\./g, '')     // Remove pontos de milhar
            .replace(',', '.')      // Troca vírgula decimal por ponto
            .trim();
          price = parseFloat(cleanPrice) || 0;
        } else if (typeof product.price === 'number') {
          price = product.price;
        }
        
        // Converter stockQuantity para número (verifica ambos os campos)
        const stock = typeof product.stockQuantity === 'number' ? product.stockQuantity : 
                     typeof product.stockQuantity === 'string' ? parseFloat(product.stockQuantity) || 0 :
                     typeof product.stock === 'number' ? product.stock :
                     typeof product.stock === 'string' ? parseFloat(product.stock) || 0 : 0;
        
        return sum + (price * stock);
      }, 0);
      
      const totalRevenue = totalValue * 0.7;
      const lowStockProducts = products.filter(p => {
        const stock = typeof p.stockQuantity === 'number' ? p.stockQuantity : 
                     typeof p.stockQuantity === 'string' ? parseFloat(p.stockQuantity) || 0 :
                     typeof p.stock === 'number' ? p.stock :
                     typeof p.stock === 'string' ? parseFloat(p.stock) || 0 : 0;
        return stock < 10;
      }).length;
      
      const averagePrice = totalProducts > 0 
        ? products.reduce((sum, p) => {
            let price = 0;
            if (typeof p.price === 'string') {
              let cleanPrice = p.price
                .replace(/R\$\s?/g, '')
                .replace(/\./g, '')
                .replace(',', '.')
                .trim();
              price = parseFloat(cleanPrice) || 0;
            } else if (typeof p.price === 'number') {
              price = p.price;
            }
            return sum + price;
          }, 0) / totalProducts 
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