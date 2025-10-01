import { useState, useEffect } from 'react';
import { productService, Product, PaginatedResponse } from '@/services/productService';
import { categoryService } from '@/services/categoryService';

export function useProducts(pageNumber = 1, pageSize = 10, search = '', categoryId = '') {
  const [products, setProducts] = useState<PaginatedResponse<Product>>({
    items: [],
    pageNumber: 1,
    totalPages: 0,
    totalCount: 0,
    pageSize: 10,
    hasPreviousPage: false,
    hasNextPage: false
  });
  const [isLoading, setIsLoading] = useState(true);
  const [error] = useState<string | null>(null);

  // ✅ FUNÇÃO DE ENRIQUECIMENTO CORRIGIDA
  const enrichProductsWithCategories = async (products: Product[]) => {
    try {
      console.log('🔍 Produtos antes do enriquecimento:', products.map(p => ({ name: p.name, categoryId: p.categoryId, categoryName: p.categoryName })));
      
      const categories = await categoryService.getAll();
      console.log('📂 Categorias carregadas:', categories);
      
      const categoryMap = new Map(categories.map(cat => [cat.id, cat.name]));
      console.log('🗺️ Mapa de categorias:', Array.from(categoryMap.entries()));
      
      const enrichedProducts = products.map(product => {
        const categoryName = categoryMap.get(product.categoryId) || 'Sem categoria';
        console.log(`📦 Produto: ${product.name}, categoryId: ${product.categoryId}, categoryName encontrado: ${categoryName}`);
        
        return {
          ...product,
          categoryName: product.categoryName || categoryName
        };
      });
      
      console.log('✨ Produtos após enriquecimento:', enrichedProducts.map(p => ({ name: p.name, categoryName: p.categoryName })));
      return enrichedProducts;
      
    } catch (error) {
      console.error('❌ Erro ao enriquecer produtos com categorias:', error);
      return products;
    }
  };
  // No useProducts.ts, DEPOIS da chamada da API:

  const fetchProducts = async () => {
    try {
      setIsLoading(true);
            
      console.log('🔍 === USEPRODUCTS DEBUG ===');
      console.log('📂 categoryId recebido:', categoryId);
      
      // ✅ CHAME A API SEM FILTRO PRIMEIRO
      const data = await productService.getProducts(pageNumber, pageSize, search);
      
      console.log('📦 Dados SEM filtro:', data.items.length);
      
      // ✅ FILTRE LOCALMENTE SE CATEGORIA SELECIONADA
      let filteredItems = data.items;
      if (categoryId && categoryId !== '') {
        filteredItems = data.items.filter(product => product.categoryId === categoryId);
        console.log('🔍 Produtos após filtro local:', filteredItems.length);
      }
      
      const enrichedProducts = await enrichProductsWithCategories(filteredItems);
      
      setProducts({
        ...data,
        items: enrichedProducts,
        totalCount: filteredItems.length // ✅ AJUSTE O TOTAL
      });
      
    } catch (err: any) {
      console.error(err.message || 'Erro ao carregar produtos');
    } finally {
      setIsLoading(false);
    }
  };
  
  

  useEffect(() => {
    fetchProducts();
  }, [pageNumber, pageSize, search, categoryId]);

  return {
    products,
    isLoading,
    error,
    refetch: fetchProducts
  };
}
