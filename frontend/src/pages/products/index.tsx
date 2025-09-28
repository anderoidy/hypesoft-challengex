'use client'
import React, { useState, useEffect } from 'react';
import { FiSearch, FiPlus, FiFilter, FiDownload, FiMoreVertical, FiEdit2, FiTrash2 } from 'react-icons/fi';
import { useRouter } from 'next/router';
import { MainLayout } from '@/components/layout/MainLayout';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { ProtectedRoute } from '@/components/ProtectedRoute';
import { useProducts } from '@/hooks/useProducts';
import { Product, productService } from '@/services/productService';
import { categoryService, Category } from '@/services/categoryService';
import { FileDown, Loader2 } from 'lucide-react';

import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';

// ✅ Sistema de Toast nativo (substituindo react-toastify)
interface ToastProps {
  message: string;
  type: 'success' | 'error' | 'info';
  onClose: () => void;
}

const Toast: React.FC<ToastProps> = ({ message, type, onClose }) => {
  useEffect(() => {
    const timer = setTimeout(onClose, 5000);
    return () => clearTimeout(timer);
  }, [onClose]);

  const bgColor = type === 'success' ? 'bg-green-500' : type === 'error' ? 'bg-red-500' : 'bg-blue-500';

  return (
    <div className={`fixed top-4 right-4 ${bgColor} text-white px-6 py-3 rounded-lg shadow-lg z-50 transition-all duration-300`}>
      <div className="flex items-center justify-between">
        <span>{message}</span>
        <button onClick={onClose} className="ml-4 text-white hover:text-gray-200">
          ×
        </button>
      </div>
    </div>
  );
};

interface ToastState {
  id: number;
  message: string;
  type: 'success' | 'error' | 'info';
}

const useToast = () => {
  const [toasts, setToasts] = useState<ToastState[]>([]);

  const toast = {
    success: (message: string) => {
      const id = Date.now();
      setToasts(prev => [...prev, { id, message, type: 'success' }]);
    },
    error: (message: string) => {
      const id = Date.now();
      setToasts(prev => [...prev, { id, message, type: 'error' }]);
    },
    info: (message: string) => {
      const id = Date.now();
      setToasts(prev => [...prev, { id, message, type: 'info' }]);
    }
  };

  const removeToast = (id: number) => {
    setToasts(prev => prev.filter(toast => toast.id !== id));
  };

  return { toasts, toast, removeToast };
};

// Loading skeleton component
const ProductSkeleton = () => (
  <tr className="animate-pulse">
    <td className="px-6 py-4">
      <div className="flex items-center">
        <div className="flex-shrink-0 h-10 w-10 bg-gray-200 rounded-md"></div>
        <div className="ml-4">
          <div className="h-4 bg-gray-200 rounded w-32 mb-2"></div>
          <div className="h-3 bg-gray-200 rounded w-20"></div>
        </div>
      </div>
    </td>
    <td className="px-6 py-4"><div className="h-4 bg-gray-200 rounded w-24"></div></td>
    <td className="px-6 py-4"><div className="h-4 bg-gray-200 rounded w-20"></div></td>
    <td className="px-6 py-4"><div className="h-4 bg-gray-200 rounded w-16"></div></td>
    <td className="px-6 py-4"><div className="h-4 bg-gray-200 rounded w-24"></div></td>
    <td className="px-6 py-4"><div className="h-6 w-6 bg-gray-200 rounded"></div></td>
  </tr>
);

export default function ProductsPage() {
  const router = useRouter();
  const [currentPage, setCurrentPage] = useState(1);
  const [searchTerm, setSearchTerm] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  
  const [isExporting, setIsExporting] = useState(false);

  // ✅ Hook de Toast
  const { toasts, toast, removeToast } = useToast();

  const handleExportPdf = async () => {
    try {
      setIsExporting(true);
      console.log('🚀 Chamando backend real...');
      
      const token = localStorage.getItem('accessToken');
      console.log('🔑 Token encontrado:', token ? 'Sim' : 'Não');
      
      // ✅ USAR A URL QUE FUNCIONAVA
      const response = await fetch('/api/Products/report/pdf', {  // ← P maiúsculo
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Accept': 'application/pdf',
        },
      });
      
      console.log('📊 Status:', response.status);
      
      if (!response.ok) {
        throw new Error(`Erro ${response.status}: ${response.statusText}`);
      }
      
      const blob = await response.blob();
      console.log('📦 Blob:', blob.size, 'bytes');
      
      // Download
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `relatorio-produtos-${Date.now()}.pdf`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      window.URL.revokeObjectURL(url);
      
      toast.success('PDF baixado com sucesso!');
      
    } catch (error) {
      console.error('❌ Erro:', error);
      const errorMessage = error instanceof Error ? error.message : 'Erro desconhecido';
      toast.error(`Erro: ${errorMessage}`);
    } finally {
      setIsExporting(false);
    }
  };
  
  // ✅ NOVO ESTADO PARA FILTRO DE CATEGORIA
  const [selectedCategoryId, setSelectedCategoryId] = useState<string>('');
  const [isFilterOpen, setIsFilterOpen] = useState(false);
  
  // ✅ HOOK ATUALIZADO COM FILTRO DE CATEGORIA
  const { products, isLoading, error, refetch } = useProducts(
    currentPage, 
    10, 
    debouncedSearch,
    selectedCategoryId // ← NOVO PARÂMETRO
  );

  // Estados do modal de edição
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [editingProduct, setEditingProduct] = useState<Product | null>(null);
  const [editForm, setEditForm] = useState({
    name: '',
    description: '',
    price: 0,
    sku: '',
    stockQuantity: 0
  });

  // ✅ ESTADOS PARA CRIAR PRODUTO E CATEGORIAS
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [categories, setCategories] = useState<Category[]>([]);
  const [createForm, setCreateForm] = useState({
    name: '',
    description: '',
    price: 0,
    sku: '',
    stockQuantity: 0,
    categoryId: ''
  });

  // ✅ NOVOS ESTADOS PARA MODAL DE CATEGORIA ANINHADO
  const [isCategoryModalOpen, setIsCategoryModalOpen] = useState(false);
  const [newCategoryForm, setNewCategoryForm] = useState({
    name: '',
    description: ''
  });

  // ✅ CARREGAR CATEGORIAS SEMPRE (NÃO SÓ NO MODAL)
  useEffect(() => {
    const loadCategories = async () => {
      try {
        const categoriesData = await categoryService.getAll();
        setCategories(categoriesData);
      } catch (error) {
        console.error('Erro ao carregar categorias:', error);
      }
    };
    
    loadCategories(); // ← CARREGA SEMPRE PARA USAR NA TABELA
  }, []);

  // ✅ RECARREGAR CATEGORIAS QUANDO MODAL ABRE (PARA PEGAR NOVAS)
  useEffect(() => {
    const loadCategories = async () => {
      try {
        const categoriesData = await categoryService.getAll();
        setCategories(categoriesData);
      } catch (error) {
        console.error('Erro ao carregar categorias:', error);
      }
    };
    
    if (isCreateModalOpen) {
      loadCategories();
    }
  }, [isCreateModalOpen]);

  // Debounce search
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearch(searchTerm);
      setCurrentPage(1);
    }, 300);

    return () => clearTimeout(timer);
  }, [searchTerm]);

  // ✅ FUNÇÃO PARA RESETAR FILTROS
  const handleResetFilters = () => {
    setSelectedCategoryId('');
    setSearchTerm('');
    setCurrentPage(1);
  };

  // ✅ FUNÇÃO PARA APLICAR FILTRO DE CATEGORIA com o debug 
  const handleCategoryFilter = (categoryId: string) => {
    console.log('🔍 === CATEGORIA SELECIONADA ===');
    console.log('📂 categoryId:', categoryId);
    
    const selectedCategory = categories.find(cat => cat.id === categoryId);
    console.log('🏷️ Categoria encontrada:', selectedCategory);
    console.log('📋 Todas as categorias:', categories);
    
    setSelectedCategoryId(categoryId);
    setCurrentPage(1);
    setIsFilterOpen(false);
  };
  

  // ✅ FUNÇÃO PARA ABRIR MODAL DE CRIAR
  const handleOpenCreateModal = () => {
    setCreateForm({
      name: '',
      description: '',
      price: 0,
      sku: '',
      stockQuantity: 0,
      categoryId: ''
    });
    setIsCreateModalOpen(true);
  };

  // ✅ FUNÇÃO PARA ABRIR MODAL DE CATEGORIA ANINHADO
  const handleOpenCategoryModal = () => {
    setNewCategoryForm({ name: '', description: '' });
    setIsCategoryModalOpen(true);
  };

  // ✅ FUNÇÃO PARA CRIAR CATEGORIA DENTRO DO MODAL
  const handleCreateCategoryInModal = async () => {
    try {
      if (!newCategoryForm.name.trim()) {
        toast.error('Nome da categoria é obrigatório!');
        return;
      }

      console.log('🆕 Criando categoria:', newCategoryForm);
      
      // Criar categoria
      const newCategory = await categoryService.create({
        name: newCategoryForm.name.trim(),
        description: newCategoryForm.description?.trim() || '',
        isActive: true
      });

      console.log('✅ Categoria criada:', newCategory);

      // Recarregar categorias
      const updatedCategories = await categoryService.getAll();
      setCategories(updatedCategories);

      // Selecionar automaticamente a nova categoria
      setCreateForm({
        ...createForm,
        categoryId: newCategory.id
      });

      // Fechar modal de categoria
      setIsCategoryModalOpen(false);
      setNewCategoryForm({ name: '', description: '' });

      toast.success(`Categoria "${newCategory.name}" criada com sucesso!`);

    } catch (error: any) {
      console.error('❌ Erro ao criar categoria:', error);
      toast.error(`Erro ao criar categoria: ${error.response?.data || error.message}`);
    }
  };

  // ✅ FUNÇÃO PARA CRIAR PRODUTO
  const handleCreateProduct = async () => {
    try {
      console.log('🆕 Criando produto:', createForm);
      
      // Validação básica
      if (!createForm.name.trim()) {
        toast.error('Nome do produto é obrigatório!');
        return;
      }

      if (!createForm.categoryId) {
        toast.error('Categoria é obrigatória!');
        return;
      }

      // Preparar dados para API
      const newProduct = {
        name: createForm.name.trim(),
        description: createForm.description?.trim() || '',
        price: Number(createForm.price) || 0,
        categoryId: createForm.categoryId,
        sku: createForm.sku?.trim() || '',
        barcode: '',
        discountPrice: 0,
        stockQuantity: Number(createForm.stockQuantity) || 0,
        imageUrl: 'https://via.placeholder.com/300x300?text=Novo+Produto',
        isFeatured: false,
        isPublished: true,
        createdBy: 'anderx'
      };
      
      console.log('🚀 Dados para enviar:', newProduct);
      
      // Chamar API
      await productService.createProduct(newProduct);
      
      // Fechar modal
      setIsCreateModalOpen(false);
      
      // Atualizar lista
      await refetch();
      
      toast.success(`Produto "${createForm.name}" criado com sucesso!`);
      
    } catch (error: any) {
      console.error('❌ Erro ao criar produto:', error);
      console.error('📄 Response data:', error.response?.data);
      console.error('📊 Status:', error.response?.status);
      
      toast.error(`Erro ao criar produto: ${error.response?.data || error.message}`);
    }
  };

  const handleEdit = (product: Product) => {
    console.log('Editando produto:', product);
    
    setEditForm({
      name: product.name,
      description: product.description || '',
      price: product.price,
      sku: product.sku || '',
      stockQuantity: product.stockQuantity || 0
    });
    
    setEditingProduct(product);
    setIsEditModalOpen(true);
  };

  const handleSaveEdit = async () => {
    if (!editingProduct) return;
    
    try {
      console.log('🔍 Produto original:', editingProduct);
      console.log('📝 Formulário editado:', editForm);
      
      const updatedProduct = {
        name: editForm.name,
        description: editForm.description || '',
        price: Number(editForm.price),
        categoryId: editingProduct.categoryId,
        sku: editForm.sku || '',
        barcode: editingProduct.barcode || '',
        discountPrice: editingProduct.discountPrice === null ? undefined : editingProduct.discountPrice,
        stockQuantity: Number(editForm.stockQuantity),
        imageUrl: editingProduct.imageUrl || '',
        isFeatured: editingProduct.isFeatured || false,
        isPublished: editingProduct.isPublished !== false,
        createdBy: editingProduct.createdBy || 'anderx'
      };
      
      console.log('🚀 Dados para enviar:', updatedProduct);
      
      await productService.updateProduct(editingProduct.id!, updatedProduct);
      
      setIsEditModalOpen(false);
      setEditingProduct(null);
      refetch();
      
      toast.success(`Produto "${editForm.name}" atualizado com sucesso!`);
      
    } catch (error: any) {
      console.error('❌ Erro completo:', error);
      toast.error(`Erro ao atualizar produto: ${error.response?.data || error.message}`);
    }
  };

  const handleDelete = async (product: Product) => {
    if (confirm(`Tem certeza que deseja deletar "${product.name}"?`)) {
      try {
        await productService.deleteProduct(product.id!);
        toast.success(`Produto "${product.name}" deletado com sucesso!`);
        refetch();
      } catch (error: any) {
        console.error('Erro ao deletar produto:', error);
        toast.error(`Erro ao deletar produto: ${error.message}`);
      }
    }
  };

  const getStockStatus = (stock: number | null | undefined) => {
    if (!stock || stock === 0) {
      return { text: 'Sem estoque', class: 'bg-red-100 text-red-800' };
    } else if (stock < 10) {
      return { text: 'Estoque baixo', class: 'bg-yellow-100 text-yellow-800' };
    } else {
      return { text: 'Em estoque', class: 'bg-green-100 text-green-800' };
    }
  };

  // ✅ FUNÇÃO PARA BUSCAR NOME DA CATEGORIA
  const getCategoryName = (categoryId: string) => {
    const category = categories.find(cat => cat.id === categoryId);
    return category?.name || 'Sem categoria';
  };

  return (
    <ProtectedRoute>
      <MainLayout>
        {/* ✅ Toasts Container */}
        <div className="fixed top-4 right-4 z-50 space-y-2">
          {toasts.map(toast => (
            <Toast
              key={toast.id}
              message={toast.message}
              type={toast.type}
              onClose={() => removeToast(toast.id)}
            />
          ))}
        </div>

        <div className="p-6">
          <div className="flex flex-col md:flex-row md:items-center md:justify-between mb-6">
            <div>
              <h1 className="text-2xl font-bold text-gray-900">Produtos</h1>
              <p className="text-gray-500">
                {isLoading ? (
                  'Carregando produtos...'
                ) : (
                  `Gerencie seus produtos e estoque (${products.totalCount} produtos)`
                )}
              </p>
            </div>
            <Button 
              className="mt-4 md:mt-0"
              onClick={handleOpenCreateModal}
            >
              <FiPlus className="mr-2 h-4 w-4" />
              Adicionar Produto
            </Button>
          </div>

          {/* ✅ INDICADOR DE FILTROS ATIVOS */}
          {(selectedCategoryId || searchTerm) && (
            <div className="flex items-center gap-2 mb-4">
              <span className="text-sm text-gray-500">Filtros ativos:</span>
              {searchTerm && (
                <span className="inline-flex items-center gap-1 px-2 py-1 bg-gray-100 text-gray-800 text-xs rounded-full">
                  🔍 &quot;{searchTerm}&quot;
                  <button 
                    onClick={() => setSearchTerm('')}
                    className="hover:text-red-600"
                  >
                    ✕
                  </button>
                </span>
              )}
              {selectedCategoryId && (
                <span className="inline-flex items-center gap-1 px-2 py-1 bg-blue-100 text-blue-800 text-xs rounded-full">
                  📂 {categories.find(cat => cat.id === selectedCategoryId)?.name}
                  <button 
                    onClick={() => setSelectedCategoryId('')}
                    className="hover:text-red-600"
                  >
                    ✕
                  </button>
                </span>
              )}
              <button
                onClick={handleResetFilters}
                className="text-xs text-red-600 hover:underline"
              >
                Limpar todos
              </button>
            </div>
          )}

          {/* Error State */}
          {error && (
            <div className="mb-6 bg-red-50 border border-red-200 rounded-lg p-4">
              <p className="text-red-600">Erro ao carregar produtos: {error}</p>
              <Button 
                variant="outline" 
                size="sm" 
                onClick={refetch}
                className="mt-2"
              >
                Tentar novamente
              </Button>
            </div>
          )}

          <div className="bg-white rounded-lg shadow-sm border border-gray-200">
            <div className="p-4 border-b border-gray-200">
              <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                <div className="relative flex-1">
                  <FiSearch className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
                  <Input
                    type="text"
                    placeholder="Buscar produtos..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    className="pl-10 w-full"
                  />
                </div>
                <div className="flex gap-2">
                  {/* ✅ BOTÃO FILTRAR ATUALIZADO COM DROPDOWN */}
                  <div className="relative">
                    <Button 
                      variant="outline" 
                      onClick={() => setIsFilterOpen(!isFilterOpen)}
                      className={selectedCategoryId ? 'border-blue-500 text-blue-600' : ''}
                    >
                      <FiFilter className="mr-2 h-4 w-4" />
                      Filtrar
                      {selectedCategoryId && (
                        <span className="ml-1 bg-blue-100 text-blue-800 text-xs px-2 py-1 rounded-full">
                          {categories.find(cat => cat.id === selectedCategoryId)?.name}
                        </span>
                      )}
                    </Button>

                    {/* ✅ DROPDOWN DE FILTROS */}
                    {isFilterOpen && (
                      <div className="absolute right-0 mt-2 w-64 bg-white rounded-lg shadow-lg border border-gray-200 z-10">
                        <div className="p-4">
                          <h3 className="font-medium text-gray-900 mb-3">Filtrar por Categoria</h3>
                          
                          <div className="space-y-2 max-h-60 overflow-y-auto">
                            {/* Opção "Todas as categorias" */}
                            <button
                              onClick={() => handleCategoryFilter('')}
                              className={`w-full text-left px-3 py-2 rounded-md text-sm transition-colors ${
                                selectedCategoryId === '' 
                                  ? 'bg-blue-100 text-blue-800 font-medium' 
                                  : 'hover:bg-gray-100 text-gray-700'
                              }`}
                            >
                              📋 Todas as categorias
                            </button>
                            
                            {/* Lista de categorias */}
                            {categories.map((category) => (
                              <button
                                key={category.id}
                                onClick={() => handleCategoryFilter(category.id)}
                                className={`w-full text-left px-3 py-2 rounded-md text-sm transition-colors ${
                                  selectedCategoryId === category.id 
                                    ? 'bg-blue-100 text-blue-800 font-medium' 
                                    : 'hover:bg-gray-100 text-gray-700'
                                }`}
                              >
                                📂 {category.name}
                              </button>
                            ))}
                          </div>
                          
                          {/* Botão resetar */}
                          {(selectedCategoryId || searchTerm) && (
                            <div className="mt-3 pt-3 border-t border-gray-200">
                              <button
                                onClick={handleResetFilters}
                                className="w-full px-3 py-2 text-sm text-red-600 hover:bg-red-50 rounded-md transition-colors"
                              >
                                🗑️ Limpar todos os filtros
                              </button>
                            </div>
                          )}
                        </div>
                      </div>
                    )}
                  </div>
                  <button 
                    onClick={handleExportPdf}
                    disabled={isExporting}
                    className="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 disabled:opacity-50"
                  >
                    {isExporting ? 'Exportando...' : 'Exportar PDF'}
                  </button>

                  
                </div>
              </div>
            </div>

            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Produto
                    </th>
                    <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Categoria
                    </th>
                    <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Preço
                    </th>
                    <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Estoque
                    </th>
                    <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Status
                    </th>
                    <th scope="col" className="relative px-6 py-3">
                      <span className="sr-only">Ações</span>
                    </th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {isLoading ? (
                    Array.from({ length: 5 }).map((_, i) => <ProductSkeleton key={i} />)
                  ) : products.items.length === 0 ? (
                    <tr>
                      <td colSpan={6} className="px-6 py-12 text-center">
                        <div className="text-gray-500">
                          <FiSearch className="mx-auto h-12 w-12 text-gray-400 mb-4" />
                          <h3 className="text-sm font-semibold text-gray-900 mb-1">
                            Nenhum produto encontrado
                          </h3>
                          <p className="text-sm text-gray-500">
                            {searchTerm ? 'Tente uma busca diferente' : 'Comece criando um novo produto'}
                          </p>
                        </div>
                      </td>
                    </tr>
                  ) : (
                    products.items.map((product) => {
                      const stockStatus = getStockStatus(product.stockQuantity);
                      
                      return (
                        <tr key={product.id} className="hover:bg-gray-50">
                          <td className="px-6 py-4 whitespace-nowrap">
                            <div className="flex items-center">
                              <div className="flex-shrink-0 h-10 w-10 rounded-md overflow-hidden bg-gray-100">
                                {product.imageUrl ? (
                                  <img 
                                    className="h-full w-full object-cover" 
                                    src={product.imageUrl} 
                                    alt={product.name}
                                    onError={(e) => {
                                      e.currentTarget.src = 'https://via.placeholder.com/40x40?text=No+Image';
                                    }}
                                  />
                                ) : (
                                  <div className="h-full w-full bg-gray-200 flex items-center justify-center">
                                    <span className="text-xs text-gray-400">No Image</span>
                                  </div>
                                )}
                              </div>
                              <div className="ml-4">
                                <div className="text-sm font-medium text-gray-900">{product.name}</div>
                                <div className="text-sm text-gray-500">{product.sku || 'Sem SKU'}</div>
                              </div>
                            </div>
                          </td>
                          {/* ✅ COLUNA CATEGORIA CORRIGIDA */}
                          <td className="px-6 py-4 whitespace-nowrap">
                            <div className="text-sm text-gray-900">
                              {/*{product.categoryName || 'Sem categoria'} */}
                              {/* ✅ DEBUG TEMPORÁRIO */}
                              {product.categoryName ? `✅ ${product.categoryName}` : `❌ categoryId: ${product.categoryId}`}

                            </div>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap">
                            <div className="text-sm text-gray-900">
                              {new Intl.NumberFormat('pt-BR', {
                                style: 'currency',
                                currency: 'BRL'
                              }).format(product.price)}
                            </div>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap">
                            <div className="text-sm text-gray-900">
                              {product.stockQuantity ?? 0} unidades
                            </div>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap">
                            <div className="flex items-center space-x-2">
                              <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${stockStatus.class}`}>
                                {stockStatus.text}
                              </span>
                              {product.isFeatured && (
                                <Badge variant="outline" className="border-blue-200 text-blue-800 bg-blue-50">
                                  Destaque
                                </Badge>
                              )}
                              {!product.isPublished && (
                                <Badge variant="outline" className="border-gray-200 text-gray-500 bg-gray-50">
                                  Rascunho
                                </Badge>
                              )}
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
                                <DropdownMenuItem onClick={() => handleEdit(product)}>
                                  <FiEdit2 className="mr-2 h-4 w-4" />
                                  <span>Editar</span>
                                </DropdownMenuItem>
                                <DropdownMenuItem 
                                  className="text-red-600"
                                  onClick={() => handleDelete(product)}
                                >
                                  <FiTrash2 className="mr-2 h-4 w-4" />
                                  <span>Excluir</span>
                                </DropdownMenuItem>
                              </DropdownMenuContent>
                            </DropdownMenu>
                          </td>
                        </tr>
                      );
                    })
                  )}
                </tbody>
              </table>
            </div>

            {/* Paginação */}
            {!isLoading && products.totalPages > 1 && (
              <div className="bg-white px-4 py-3 flex items-center justify-between border-t border-gray-200 sm:px-6">
                <div className="flex-1 flex justify-between sm:hidden">
                  <button 
                    onClick={() => setCurrentPage(currentPage - 1)}
                    disabled={!products.hasPreviousPage}
                    className="relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50"
                  >
                    Anterior
                  </button>
                  <button 
                    onClick={() => setCurrentPage(currentPage + 1)}
                    disabled={!products.hasNextPage}
                    className="ml-3 relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50"
                  >
                    Próximo
                  </button>
                </div>
                <div className="hidden sm:flex-1 sm:flex sm:items-center sm:justify-between">
                  <div>
                    <p className="text-sm text-gray-700">
                      Mostrando <span className="font-medium">{Math.min(products.pageSize, products.totalCount)}</span> de{' '}
                      <span className="font-medium">{products.totalCount}</span> produtos
                    </p>
                  </div>
                  <div>
                    <span className="text-sm text-gray-700">
                      Página {currentPage} de {products.totalPages}
                    </span>
                    <div className="ml-4 inline-flex space-x-2">
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => setCurrentPage(currentPage - 1)}
                        disabled={!products.hasPreviousPage}
                      >
                        Anterior
                      </Button>
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => setCurrentPage(currentPage + 1)}
                        disabled={!products.hasNextPage}
                      >
                        Próximo
                      </Button>
                    </div>
                  </div>
                </div>
              </div>
            )}
          </div>

          {/* ✅ MODAL DE CRIAR PRODUTO COM MODAL ANINHADO */}
          {isCreateModalOpen && (
            <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
              <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-md mx-4 border dark:border-gray-700">
                <h2 className="text-xl font-bold text-gray-900 dark:text-white mb-4">
                  Criar Novo Produto
                </h2>
                
                <div className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-200 mb-1">
                      Nome *
                    </label>
                    <Input
                      value={createForm.name}
                      onChange={(e) => setCreateForm({...createForm, name: e.target.value})}
                      placeholder="Nome do produto"
                      className="text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 bg-white dark:bg-gray-700 border-gray-300 dark:border-gray-600"
                    />
                  </div>
                  
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-200 mb-1">
                      Descrição
                    </label>
                    <Input
                      value={createForm.description}
                      onChange={(e) => setCreateForm({...createForm, description: e.target.value})}
                      placeholder="Descrição do produto"
                      className="text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 bg-white dark:bg-gray-700 border-gray-300 dark:border-gray-600"
                    />
                  </div>
                  
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 dark:text-gray-200 mb-1">
                        Preço
                      </label>
                      <Input
                        type="number"
                        step="0.01"
                        value={createForm.price}
                        onChange={(e) => setCreateForm({...createForm, price: parseFloat(e.target.value) || 0})}
                        placeholder="0,00"
                        className="text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 bg-white dark:bg-gray-700 border-gray-300 dark:border-gray-600"
                      />
                    </div>
                    
                    <div>
                      <label className="block text-sm font-medium text-gray-700 dark:text-gray-200 mb-1">
                        Estoque
                      </label>
                      <Input
                        type="number"
                        value={createForm.stockQuantity}
                        onChange={(e) => setCreateForm({...createForm, stockQuantity: parseInt(e.target.value) || 0})}
                        placeholder="Quantidade"
                        className="text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 bg-white dark:bg-gray-700 border-gray-300 dark:border-gray-600"
                      />
                    </div>
                  </div>
                  
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-200 mb-1">
                      SKU
                    </label>
                    <Input
                      value={createForm.sku}
                      onChange={(e) => setCreateForm({...createForm, sku: e.target.value})}
                      placeholder="SKU do produto"
                      className="text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 bg-white dark:bg-gray-700 border-gray-300 dark:border-gray-600"
                    />
                  </div>

                  {/* ✅ CAMPO CATEGORIA COM MODAL ANINHADO */}
                  <div>
                    <label htmlFor="categoryId" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                      Categoria *
                    </label>
                    <select
                      id="categoryId"
                      name="categoryId"
                      value={createForm.categoryId || ''}
                      onChange={(e) => setCreateForm({...createForm, categoryId: e.target.value})}
                      className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
                      required
                    >
                      <option value="">Selecione uma categoria</option>
                      {categories.map((category) => (
                        <option key={category.id} value={category.id}>
                          {category.name}
                        </option>
                      ))}
                    </select>
                    
                    {/* ✅ BOTÃO PARA MODAL ANINHADO */}
                    <div className="mt-2">
                      <button
                        type="button"
                        onClick={handleOpenCategoryModal}
                        className="text-sm text-blue-600 hover:text-blue-800 dark:text-blue-400 dark:hover:text-blue-200"
                      >
                        + Não encontrou? Criar nova categoria
                      </button>
                    </div>
                  </div>
                </div>
                
                <div className="flex gap-3 mt-6">
                  <Button
                    onClick={() => setIsCreateModalOpen(false)}
                    variant="outline"
                    className="flex-1 border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-200 hover:bg-gray-50 dark:hover:bg-gray-700"
                  >
                    Cancelar
                  </Button>
                  <Button
                    onClick={handleCreateProduct}
                    className="flex-1"
                  >
                    Criar Produto
                  </Button>
                </div>
              </div>
            </div>
          )}

          {/* ✅ MODAL DE CRIAR CATEGORIA ANINHADO */}
          {isCategoryModalOpen && (
            <div className="fixed inset-0 bg-black bg-opacity-75 flex items-center justify-center z-[60]">
              <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-md mx-4 border dark:border-gray-700">
                <h3 className="text-lg font-bold text-gray-900 dark:text-white mb-4">
                  Criar Nova Categoria
                </h3>
                
                <div className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-200 mb-1">
                      Nome da Categoria *
                    </label>
                    <Input
                      value={newCategoryForm.name}
                      onChange={(e) => setNewCategoryForm({...newCategoryForm, name: e.target.value})}
                      placeholder="Nome da categoria"
                      className="text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 bg-white dark:bg-gray-700 border-gray-300 dark:border-gray-600"
                    />
                  </div>
                  
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-200 mb-1">
                      Descrição
                    </label>
                    <Input
                      value={newCategoryForm.description}
                      onChange={(e) => setNewCategoryForm({...newCategoryForm, description: e.target.value})}
                      placeholder="Descrição da categoria"
                      className="text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 bg-white dark:bg-gray-700 border-gray-300 dark:border-gray-600"
                    />
                  </div>
                </div>
                
                <div className="flex gap-3 mt-6">
                  <Button
                    onClick={() => setIsCategoryModalOpen(false)}
                    variant="outline"
                    className="flex-1 border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-200 hover:bg-gray-50 dark:hover:bg-gray-700"
                  >
                    Cancelar
                  </Button>
                  <Button
                    onClick={handleCreateCategoryInModal}
                    className="flex-1"
                  >
                    Criar Categoria
                  </Button>
                </div>
              </div>
            </div>
          )}

          {/* ✅ MODAL DE EDITAR PRODUTO (MANTIDO ORIGINAL) */}
          {isEditModalOpen && (
            <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
              <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-md mx-4 border dark:border-gray-700">
                <h2 className="text-xl font-bold text-gray-900 dark:text-white mb-4">
                  Editar Produto
                </h2>
                
                <div className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-200 mb-1">
                      Nome
                    </label>
                    <Input
                      value={editForm.name}
                      onChange={(e) => setEditForm({...editForm, name: e.target.value})}
                      placeholder="Nome do produto"
                      className="text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 bg-white dark:bg-gray-700 border-gray-300 dark:border-gray-600"
                    />
                  </div>
                  
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-200 mb-1">
                      Descrição
                    </label>
                    <Input
                      value={editForm.description}
                      onChange={(e) => setEditForm({...editForm, description: e.target.value})}
                      placeholder="Descrição do produto"
                      className="text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 bg-white dark:bg-gray-700 border-gray-300 dark:border-gray-600"
                    />
                  </div>
                  
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 dark:text-gray-200 mb-1">
                        Preço
                      </label>
                      <Input
                        type="number"
                        step="0.01"
                        value={editForm.price}
                        onChange={(e) => setEditForm({...editForm, price: parseFloat(e.target.value) || 0})}
                        placeholder="0,00"
                        className="text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 bg-white dark:bg-gray-700 border-gray-300 dark:border-gray-600"
                      />
                    </div>
                    
                    <div>
                      <label className="block text-sm font-medium text-gray-700 dark:text-gray-200 mb-1">
                        Estoque
                      </label>
                      <Input
                        type="number"
                        value={editForm.stockQuantity}
                        onChange={(e) => setEditForm({...editForm, stockQuantity: parseInt(e.target.value) || 0})}
                        placeholder="Quantidade"
                        className="text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 bg-white dark:bg-gray-700 border-gray-300 dark:border-gray-600"
                      />
                    </div>
                  </div>
                  
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-200 mb-1">
                      SKU
                    </label>
                    <Input
                      value={editForm.sku}
                      onChange={(e) => setEditForm({...editForm, sku: e.target.value})}
                      placeholder="SKU do produto"
                      className="text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 bg-white dark:bg-gray-700 border-gray-300 dark:border-gray-600"
                    />
                  </div>
                </div>
                
                <div className="flex gap-3 mt-6">
                  <Button
                    onClick={() => {
                      setIsEditModalOpen(false);
                      setEditingProduct(null);
                    }}
                    variant="outline"
                    className="flex-1 border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-200 hover:bg-gray-50 dark:hover:bg-gray-700"
                  >
                    Cancelar
                  </Button>
                  <Button
                    onClick={handleSaveEdit}
                    className="flex-1"
                  >
                    Salvar
                  </Button>
                </div>
              </div>
            </div>
          )}          
        </div>
      </MainLayout>
    </ProtectedRoute>
  );
}
