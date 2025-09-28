import { api } from './api';

export interface Product {
  id?: string;
  name: string;
  description?: string;
  price: number;
  categoryId: string;
  categoryName?: string; // ✅ ADICIONE ESTA LINHA
  sku?: string;
  barcode?: string;
  discountPrice?: number;
  stockQuantity?: number;
  imageUrl?: string;
  isFeatured?: boolean;
  isPublished?: boolean;
  createdBy?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface Category {
  id: string;
  name: string;
  description?: string;
}

export interface ApiResponse<T> {
  value: T;
  status: number;
  isSuccess: boolean;
  successMessage?: string;
  correlationId?: string;
  location?: string;
  errors?: string[];
  validationErrors?: any[];
}

export interface PaginatedResponse<T> {
  items: T[];
  pageNumber: number;
  totalPages: number;
  totalCount: number;
  pageSize: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export const productService = {
  // ✅ LISTAR PRODUTOS COM PAGINAÇÃO E FILTRO DE CATEGORIA
  async getProducts(
    pageNumber = 1, 
    pageSize = 10,     
    search?: string,
    categoryId?: string
  ): Promise<PaginatedResponse<Product>> {
    // ✅ ADICIONAR ESTES LOGS PARA DEBUG
    console.log('🔍 === PRODUCT SERVICE DEBUG ===');
    console.log('📄 pageNumber:', pageNumber);
    console.log('📦 pageSize:', pageSize);
    console.log('🔍 search:', search);
    console.log('📂 categoryId:', categoryId);
  
    const params = new URLSearchParams({
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString(),
    });
    
    if (search) {
      params.append('search', search);
    }
  
    if (categoryId) {
      params.append('categoryId', categoryId);
    }
  
    // ✅ LOG DA URL FINAL
    const finalUrl = `/Products?${params}`;
    console.log('🌐 URL final:', finalUrl);
    console.log('📋 Params:', params.toString());
  
    const response = await api.get<ApiResponse<PaginatedResponse<Product>>>(
      finalUrl
    );
    
    // ✅ LOG DA RESPOSTA
    console.log('✅ Resposta da API:', response.data);
    
    return response.data.value;
  },
  

  // Buscar produto por ID
  async getProduct(id: string): Promise<Product> {
    const response = await api.get<ApiResponse<Product>>(`/Products/${id}`);
    return response.data.value;
  },

  // Criar produto
  async createProduct(product: Omit<Product, 'id'>): Promise<Product> {
    const response = await api.post<ApiResponse<Product>>('/Products', product);
    return response.data.value;
  },

  // Atualizar produto
  async updateProduct(id: string, product: Omit<Product, 'id'>): Promise<Product> {
    console.log('🔍 === DEBUG UPDATE PRODUCT ===');
    console.log('📝 ID recebido:', id);
    console.log('📦 Produto recebido:', JSON.stringify(product, null, 2));

    // Preparar payload limpo
    const productData = {
      name: product.name || "",
      description: product.description || "",
      price: Number(product.price) || 0,
      categoryId: product.categoryId || "",
      sku: product.sku || "",
      barcode: product.barcode || "",
      discountPrice: product.discountPrice || 0,
      stockQuantity: Number(product.stockQuantity) || 0,
      imageUrl: product.imageUrl || "",
      isFeatured: Boolean(product.isFeatured),
      isPublished: Boolean(product.isPublished),
      createdBy: product.createdBy || "anderx"
    };

    console.log('🚀 Payload final:', JSON.stringify(productData, null, 2));
    console.log('🌐 URL:', `/Products/${id}`);

    try {
      const response = await api.put(`/Products/${id}`, productData);
      console.log('✅ Resposta completa da API:', response);
      console.log('📦 response.data:', response.data);
      console.log('🔍 response.status:', response.status);
      
      // PUT normalmente retorna 204 (No Content) ou apenas o ID
      if (response.status === 204 || !response.data) {
        // Se não retorna dados, retornar o produto atualizado
        console.log('ℹ️ API retornou 204 No Content - produto atualizado com sucesso');
        return { id, ...productData } as Product;
      }
      
      // Se retorna dados, verificar estrutura
      if (response.data?.value) {
        console.log('✅ Retornando response.data.value:', response.data.value);
        return response.data.value;
      } else if (response.data?.id) {
        console.log('✅ Retornando response.data (ID):', response.data);
        return { id, ...productData } as Product;
      } else {
        console.log('✅ Retornando produto construído:', { id, ...productData });
        return { id, ...productData } as Product;
      }
      
    } catch (error: any) {
      console.error('❌ Erro detalhado:');
      console.error('📊 Status:', error.response?.status);
      console.error('📄 Data:', error.response?.data);
      console.error('🔧 Config:', error.config?.data);
      console.error('🌐 URL chamada:', error.config?.url);
      throw error;
    }
  }, 

  // Deletar produto
  async deleteProduct(id: string): Promise<void> {
    await api.delete(`/Products/${id}`);
  },

  // Listar categorias (para dropdown)
  async getCategories(): Promise<Category[]> {
    const response = await api.get<ApiResponse<PaginatedResponse<Category>>>(
      '/Categories?pageSize=100'
    );
    return response.data.value.items;
  },

  exportToPdf: async (): Promise<Blob> => {
        try {
        console.log('📄 Exportando relatório PDF...');
        
        const response = await api.get('/Products/report/pdf', {
            responseType: 'blob', // ✅ IMPORTANTE: tipo blob para PDF
        });
        
        console.log('✅ PDF exportado com sucesso');
        return response.data;
        } catch (error: any) {
        console.error('❌ Erro ao exportar PDF:', error);
        throw new Error(error.response?.data?.message || 'Erro ao exportar relatório');
        }
    }
};
