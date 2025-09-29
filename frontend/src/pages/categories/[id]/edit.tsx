'use client';
export const dynamic = 'force-dynamic';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/router';
import { FiSave, FiArrowLeft } from 'react-icons/fi';
import { MainLayout } from '@/components/layout/MainLayout';
import { ProtectedRoute } from '@/components/ProtectedRoute';
import { categoryService, Category, UpdateCategoryDto } from '@/services/categoryService';

const EditCategoryPage = () => {
    const router = useRouter();
    const { id } = router.query;
    
    console.log('🔍 Router query:', router.query);
    console.log('🔍 ID extraido:', id);
    
    const [category, setCategory] = useState<Category | null>(null);
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [formData, setFormData] = useState<UpdateCategoryDto>({
      id: '',
      name: '',
      description: '',
      isActive: true
    });
  
    useEffect(() => {
      console.log('🔄 useEffect executado, id:', id);
      if (id) {
        loadCategory();
      }
    }, [id]);
  
    const loadCategory = async () => {
      try {
        console.log('🚀 Carregando categoria:', id);
        setLoading(true);
        setError(null);
        
        const data = await categoryService.getById(id as string);
        console.log('✅ Categoria carregada:', data);
        
        setCategory(data);
        setFormData({
          id: data.id,
          name: data.name,
          description: data.description || '',
          isActive: data.isActive
        });
      } catch (error) {
        console.error('❌ Erro ao carregar categoria:', error);
        setError('Nao foi possivel carregar a categoria. Tente novamente.');
      } finally {
        setLoading(false);
      }
    };
      
    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
          setSaving(true);
          setError(null);
          
          console.log('💾 Salvando alteracoes:', formData);
          await categoryService.update(formData.id, formData);
          
          console.log('✅ Categoria atualizada com sucesso');
          router.push('/categories');
        } catch (error) {
          console.error('❌ Erro ao atualizar categoria:', error);
          setError('Nao foi possivel atualizar a categoria. Tente novamente.');
        } finally {
          setSaving(false);
        }
      };
      
    if (!id) {
      console.log('⚠️ ID nao encontrado, mostrando loading...');
      return (
        <ProtectedRoute>
          <MainLayout title="Editar Categoria">
            <div className="flex justify-center py-8">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
            </div>
          </MainLayout>
        </ProtectedRoute>
      );
    }

    if (loading) {
      console.log('⏳ Carregando categoria...');
      return (
        <ProtectedRoute>
          <MainLayout title="Editar Categoria">
            <div className="flex justify-center py-8">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
            </div>
          </MainLayout>
        </ProtectedRoute>
      );
    }

    if (error) {
      console.log('❌ Erro encontrado:', error);
      return (
        <ProtectedRoute>
          <MainLayout title="Editar Categoria">
            <div className="max-w-2xl mx-auto">
              <div className="mb-6">
                <button
                  onClick={() => router.push('/categories')}
                  className="flex items-center gap-2 text-blue-600 hover:text-blue-700"
                >
                  <FiArrowLeft />
                  Voltar para Categorias
                </button>
              </div>
              
              <div className="bg-red-50 border border-red-200 rounded-lg p-4">
                <h3 className="text-red-800 font-medium mb-2">Erro</h3>
                <p className="text-red-700 mb-4">{error}</p>
                <button
                  onClick={loadCategory}
                  className="bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded-lg"
                >
                  Tentar novamente
                </button>
              </div>
            </div>
          </MainLayout>
        </ProtectedRoute>
      );
    }

    console.log('✅ Renderizando formulario com dados:', formData);
    
  return (
    <ProtectedRoute>
      <MainLayout title="Editar Categoria">
        <div className="max-w-2xl mx-auto">
          <div className="mb-6">
            <button
              onClick={() => router.push('/categories')}
              className="flex items-center gap-2 text-blue-600 hover:text-blue-700"
            >
              <FiArrowLeft />
              Voltar para Categorias
            </button>
          </div>

          <form onSubmit={handleSubmit} className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-6">
            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Nome
              </label>
              <input
                type="text"
                value={formData.name}
                onChange={(e) => setFormData({...formData, name: e.target.value})}
                className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent dark:bg-gray-700 dark:text-white"
                required
              />
            </div>

            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Descricao
              </label>
              <textarea
                value={formData.description}
                onChange={(e) => setFormData({...formData, description: e.target.value})}
                className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent dark:bg-gray-700 dark:text-white"
                rows={3}
              />
            </div>

            <div className="mb-6">
              <label className="flex items-center gap-2">
                <input
                  type="checkbox"
                  checked={formData.isActive}
                  onChange={(e) => setFormData({...formData, isActive: e.target.checked})}
                  className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                />
                <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                  Categoria Ativa
                </span>
              </label>
            </div>

            {error && (
              <div className="mb-4 bg-red-50 border border-red-200 rounded-lg p-3">
                <p className="text-red-700 text-sm">{error}</p>
              </div>
            )}

            <div className="flex gap-3">
              <button
                type="submit"
                disabled={saving}
                className="flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg transition-colors disabled:opacity-50"
              >
                <FiSave />
                {saving ? 'Salvando...' : 'Salvar'}
              </button>
              <button
                type="button"
                onClick={() => router.push('/categories')}
                className="px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors"
              >
                Cancelar
              </button>
            </div>
          </form>
        </div>
      </MainLayout>
    </ProtectedRoute>
  );
};

export default EditCategoryPage;