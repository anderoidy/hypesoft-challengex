import { api } from './api';

// Types
export interface Category {
  id: string;
  name: string;
  description?: string;
  createdAt: string;
  updatedAt?: string;
  isActive: boolean;
}

export interface CreateCategoryDto {
  name: string;
  description?: string;
  isActive?: boolean;
  createdBy?: string;
}

export interface UpdateCategoryDto {
  id: string;
  name: string;
  description?: string;
  isActive?: boolean;
}

// ✅ MOCK TEMPORÁRIO - Para testar a interface
export const categoryService = {
  // Listar todas as categorias (MOCK + API)
  async getAll(): Promise<Category[]> {
    try {
      console.log('🚀 Tentando buscar da API...');
      const response = await api.get('/Categories');
      console.log('📦 Resposta da API:', response.data);
      
      // Tenta extrair dados da API
      if (response.data?.value?.items && Array.isArray(response.data.value.items)) {
        const apiCategories = response.data.value.items.map((item: any) => ({
          id: item.id,
          name: item.name,
          description: item.description || '',
          IsActive: item.IsActive || false,
          createdAt: new Date().toISOString()
        }));
        console.log('✅ Categorias da API:', apiCategories);
        return apiCategories;
      }
      
      throw new Error('Formato de resposta inesperado');
      
    } catch (error) {
      console.warn('⚠️ Falha na API, usando dados mock:', error);
      
      // ✅ FALLBACK - Dados mock para testar
      return [
        {
          id: '1',
          name: 'Eletrônicos',
          description: 'Smartphones, tablets, notebooks e acessórios',
          isActive: true,
          createdAt: new Date().toISOString()
        },
        {
          id: '2',
          name: 'Casa e Decoração',
          description: 'Móveis, decoração e utensílios domésticos',
          isActive: true,
          createdAt: new Date().toISOString()
        },
        {
          id: '3',
          name: 'Esportes',
          description: 'Equipamentos esportivos e fitness',
          isActive: true,
          createdAt: new Date().toISOString()
        }
      ];
    }
  },

  // Criar nova categoria (MOCK + API)
  async create(data: CreateCategoryDto): Promise<Category> {
    try {
      console.log('🚀 Criando categoria na API...');
      const requestBody = {
        name: data.name,
        description: data.description || "",
        isActive: data.isActive ?? true,
        createdBy: data.createdBy || "Admin User"
      };
      
      const response = await api.post('/Categories', requestBody);
      console.log('✅ Categoria criada na API:', response.data);
      return response.data;
      
    } catch (error) {
      console.warn('⚠️ Falha na API, simulando criação:', error);
      
      // ✅ FALLBACK - Mock de criação
      const mockCategory = {
        id: `mock_${Date.now()}`,
        name: data.name,
        description: data.description || '',
        isActive: true,
        createdAt: new Date().toISOString()
      };
      
      console.log('✅ Categoria mock criada:', mockCategory);
      return mockCategory;
    }
  },

  // Buscar por ID
  async getById(id: string): Promise<Category> {
    const categories = await this.getAll();
    return categories.find(c => c.id === id) || categories[0];
  },

  // Atualizar
async update(id: string, data: UpdateCategoryDto): Promise<Category> {
  try {
    console.log('🔧 Atualizando categoria na API:', { id, data });
    
    const requestBody = {
      id: id,
      name: data.name,
      description: data.description || "",
      isActive: data.isActive ?? true,
      modifiedBy: "Admin User"
    };
    
    const response = await api.put(`/Categories/${id}`, requestBody);
    console.log('✅ Categoria atualizada na API:', response.data);
    return response.data;
    
  } catch (error: any) {
    console.error('❌ Erro ao atualizar categoria:', error);
    throw new Error(error.response?.data?.message || 'Erro ao atualizar categoria');
  }
},

  // Deletar
  delete: async (id: string): Promise<void> => {
    try {
      console.log('🗑️ Deletando categoria:', id);
      
      const response = await api.delete(`Categories/${id}`);
      
      console.log('✅ Categoria deletada com sucesso');
      return response.data;
    } catch (error: any) {
      console.error('❌ Erro ao deletar categoria:', error);
      
      if (error.response?.status === 400) {
        throw new Error('Não é possível excluir categoria que possui produtos vinculados');
      }
      
      throw new Error(error.response?.data?.message || 'Erro ao deletar categoria');
    }
  }
};

export default categoryService;