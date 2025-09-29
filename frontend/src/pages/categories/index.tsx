'use client';
import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { FiPlus, FiEdit2, FiTrash2, FiTag, FiAlertTriangle } from 'react-icons/fi';
import { MainLayout } from '@/components/layout/MainLayout';
import { ProtectedRoute } from '@/components/ProtectedRoute';
import { categoryService, Category } from '@/services/categoryService';

const CategoriesPage = () => {
  const router = useRouter();
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [deleteModal, setDeleteModal] = useState<{show: boolean, category: Category | null}>({
    show: false,
    category: null
  });
  
  // ✅ ESTADOS PARA DELETE
  const [isDeleting, setIsDeleting] = useState(false);
  const [deleteError, setDeleteError] = useState<string | null>(null);

  // Carregar categorias
  useEffect(() => {
    loadCategories();
  }, []);

  const loadCategories = async () => {
    try {
      setLoading(true);
      const data = await categoryService.getAll();
      setCategories(data);
    } catch (error) {
      console.error('Erro ao carregar categorias:', error);
    } finally {
      setLoading(false);
    }
  };

  // ✅ FUNÇÃO DE DELETE MELHORADA
  const handleDelete = async () => {
    if (!deleteModal.category) return;
    
    try {
      setIsDeleting(true);
      setDeleteError(null);
      
      console.log('🗑️ Deletando categoria:', deleteModal.category.name);
      
      // ✅ CHAMA A API DE DELETE
      await categoryService.delete(deleteModal.category.id);
      
      console.log('✅ Categoria deletada com sucesso');
      
      // ✅ FECHA O MODAL
      setDeleteModal({show: false, category: null});
      
      // ✅ RECARREGA A LISTA
      await loadCategories();
      
      // ✅ FEEDBACK DE SUCESSO
      alert('✅ Categoria excluída com sucesso!');
      
    } catch (error: any) {
      console.error('❌ Erro ao deletar categoria:', error);
      setDeleteError(error.message || 'Erro ao deletar categoria');
      
      // ✅ FEEDBACK DE ERRO
      alert(`❌ Erro ao excluir categoria:\n\n${error.message || 'Erro desconhecido'}`);
      
    } finally {
      setIsDeleting(false);
    }
  };

  // ✅ FECHAR MODAL COM RESET
  const closeDeleteModal = () => {
    if (!isDeleting) { // Só fecha se não estiver deletando
      setDeleteModal({show: false, category: null});
      setDeleteError(null);
    }
  };

  return (
    <ProtectedRoute>
      <MainLayout title="Categorias">
        {/* Header */}
        <div className="flex justify-between items-center mb-6">
          <div>
            <h1 className="text-2xl font-bold text-gray-900 dark:text-white flex items-center gap-2">
              <FiTag />
              Categorias
            </h1>
            <p className="text-gray-500 dark:text-gray-400">
              Gerencie as categorias dos seus produtos ({categories.length} categorias)
            </p>
          </div>
          <button
            onClick={() => router.push('/categories/create')}
            className="flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg transition-colors"
          >
            <FiPlus />
            Nova Categoria
          </button>
        </div>

        {/* Loading */}
        {loading && (
          <div className="flex justify-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
          </div>
        )}

        {/* Lista de Categorias */}
        {!loading && (
          <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 overflow-hidden">
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
                <thead className="bg-gray-50 dark:bg-gray-900">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      Nome
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      Descrição
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      Status
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      Criado em
                    </th>
                    <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      Ações
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
                  {categories.map((category) => (
                    <tr key={category.id} className="hover:bg-gray-50 dark:hover:bg-gray-700">
                      <td className="px-6 py-4">
                        <div className="text-sm font-medium text-gray-900 dark:text-white">
                          {category.name}
                        </div>
                      </td>
                      <td className="px-6 py-4">
                        <div className="text-sm text-gray-500 dark:text-gray-400">
                          {category.description || '-'}
                        </div>
                      </td>
                      <td className="px-6 py-4">
                        <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                          category.isActive 
                            ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200' 
                            : 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200'
                        }`}>
                          {category.isActive ? 'Ativo' : 'Inativo'}
                        </span>
                      </td>
                      <td className="px-6 py-4 text-sm text-gray-500 dark:text-gray-400">
                        {new Date(category.createdAt).toLocaleDateString('pt-BR')}
                      </td>
                      <td className="px-6 py-4 text-right">
                        <div className="flex items-center justify-end gap-2">
                          <button
                            onClick={() => router.push(`/categories/${category.id}/edit`)}
                            className="text-blue-600 hover:text-blue-700 dark:text-blue-400 p-1"
                            title="Editar"
                          >
                            <FiEdit2 size={16} />
                          </button>
                          <button
                            onClick={() => setDeleteModal({show: true, category})}
                            className="text-red-600 hover:text-red-700 dark:text-red-400 p-1"
                            title="Deletar"
                          >
                            <FiTrash2 size={16} />
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {/* Empty State */}
            {categories.length === 0 && !loading && (
              <div className="text-center py-12">
                <FiTag className="mx-auto h-12 w-12 text-gray-400" />
                <h3 className="mt-2 text-sm font-medium text-gray-900 dark:text-white">
                  Nenhuma categoria encontrada
                </h3>
                <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">
                  Comece criando uma nova categoria
                </p>
                <button
                  onClick={() => router.push('/categories/create')}
                  className="mt-3 bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg"
                >
                  Nova Categoria
                </button>
              </div>
            )}
          </div>
        )}

        {/* ✅ MODAL DE CONFIRMAÇÃO MELHORADO */}
        {deleteModal.show && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div className="bg-white dark:bg-gray-800 p-6 rounded-lg max-w-md w-full mx-4">
              <div className="flex items-center gap-3 mb-4">
                <div className="flex-shrink-0">
                  <FiAlertTriangle className="h-6 w-6 text-red-600" />
                </div>
                <h3 className="text-lg font-medium text-gray-900 dark:text-white">
                  Confirmar Exclusão
                </h3>
              </div>
              
              <p className="text-gray-500 dark:text-gray-400 mb-6">
              Tem certeza que deseja excluir a categoria <strong>&quot;{deleteModal.category?.name}&quot;</strong>?
              </p>
              
              <div className="bg-yellow-50 dark:bg-yellow-900/20 border border-yellow-200 dark:border-yellow-800 rounded-md p-3 mb-6">
                <p className="text-sm text-yellow-800 dark:text-yellow-200">
                  ⚠️ Esta ação não pode ser desfeita! Se houver produtos vinculados a esta categoria, a exclusão falhará.
                </p>
              </div>

              {/* ✅ MOSTRAR ERRO SE HOUVER */}
              {deleteError && (
                <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-md p-3 mb-6">
                  <p className="text-sm text-red-800 dark:text-red-200">
                    ❌ {deleteError}
                  </p>
                </div>
              )}
              
              <div className="flex justify-end gap-3">
                <button
                  onClick={closeDeleteModal}
                  disabled={isDeleting}
                  className="px-4 py-2 text-gray-600 dark:text-gray-400 hover:text-gray-800 dark:hover:text-gray-200 disabled:opacity-50"
                >
                  Cancelar
                </button>
                <button
                  onClick={handleDelete}
                  disabled={isDeleting}
                  className="px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg disabled:opacity-50 flex items-center gap-2"
                >
                  {isDeleting ? (
                    <>
                      <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                      Excluindo...
                    </>
                  ) : (
                    <>
                      <FiTrash2 size={16} />
                      Excluir
                    </>
                  )}
                </button>
              </div>
            </div>
          </div>
        )}
      </MainLayout>
    </ProtectedRoute>
  );
};

export default CategoriesPage;