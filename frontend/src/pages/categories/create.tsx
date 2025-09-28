'use client';
import React, { useState } from 'react';
import { useRouter } from 'next/navigation';
import { FiSave, FiArrowLeft, FiTag } from 'react-icons/fi';
import { MainLayout } from '@/components/layout/MainLayout';
import { ProtectedRoute } from '@/components/ProtectedRoute';
import { categoryService, CreateCategoryDto } from '@/services/categoryService';

const CreateCategoryPage = () => {
  const router = useRouter();
  const [loading, setLoading] = useState(false);
  const [formData, setFormData] = useState<CreateCategoryDto>({
    name: '',
    description: '',
    isActive: true,
    createdBy: ''
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!formData.name.trim()) {
      alert('Nome é obrigatório');
      return;
    }

    try {
      setLoading(true);
      await categoryService.create(formData);
      router.push('/categories');
    } catch (error) {
      console.error('Erro ao criar categoria:', error);
      alert('Erro ao criar categoria');
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleCheckboxChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, checked } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: checked
    }));
  };

  return (
    <ProtectedRoute>
      <MainLayout title="Nova Categoria">
        {/* Header */}
        <div className="flex items-center gap-4 mb-6">
          <button
            onClick={() => router.back()}
            className="flex items-center gap-2 text-gray-600 dark:text-gray-400 hover:text-gray-800 dark:hover:text-gray-200"
          >
            <FiArrowLeft />
            Voltar
          </button>
          <div>
            <h1 className="text-2xl font-bold text-gray-900 dark:text-white flex items-center gap-2">
              <FiTag />
              Nova Categoria
            </h1>
            <p className="text-gray-500 dark:text-gray-400">
              Cadastre uma nova categoria para organizar seus produtos
            </p>
          </div>
        </div>

        {/* Formulário */}
        <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-6 max-w-2xl">
          <form onSubmit={handleSubmit} className="space-y-6">
            {/* Nome */}
            <div>
              <label htmlFor="name" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Nome da Categoria *
              </label>
              <input
                type="text"
                id="name"
                name="name"
                value={formData.name}
                onChange={handleChange}
                placeholder="Ex: Eletrônicos, Roupas, Casa..."
                className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
                required
              />
            </div>

            {/* Descrição */}
            <div>
              <label htmlFor="description" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Descrição
              </label>
              <textarea
                id="description"
                name="description"
                value={formData.description}
                onChange={handleChange}
                placeholder="Descreva o tipo de produtos desta categoria..."
                rows={3}
                className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
              />
            </div>
            {/*campo que faltava */} 
            <div>
            <label htmlFor="createdBy" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Criado por
            </label>
            <input
                type="text"
                id="createdBy"
                name="createdBy"
                value={formData.createdBy}
                onChange={handleChange}
                placeholder="Nome do usuário"
                className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
                required
            />
            </div>

            {/* Status Ativo */}
            <div className="flex items-center">
              <input
                type="checkbox"
                id="isActive"
                name="isActive"
                checked={formData.isActive}
                onChange={handleCheckboxChange}
                className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
              />
              <label htmlFor="isActive" className="ml-2 block text-sm text-gray-900 dark:text-white">
                Categoria ativa
              </label>
            </div>

            {/* Botões */}
            <div className="flex justify-end gap-3 pt-6">
              <button
                type="button"
                onClick={() => router.back()}
                className="px-6 py-2 text-gray-600 dark:text-gray-400 hover:text-gray-800 dark:hover:text-gray-200 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700"
              >
                Cancelar
              </button>
              <button
                type="submit"
                disabled={loading}
                className="flex items-center gap-2 px-6 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {loading ? (
                  <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                ) : (
                  <FiSave />
                )}
                {loading ? 'Salvando...' : 'Salvar Categoria'}
              </button>
            </div>
          </form>
        </div>

        {/* Preview */}
        <div className="mt-6 bg-gray-50 dark:bg-gray-900 rounded-xl p-6 max-w-2xl">
          <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">Preview</h3>
          <div className="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg p-4">
            <div className="flex items-center justify-between">
              <div>
                <h4 className="font-medium text-gray-900 dark:text-white">
                  {formData.name || 'Nome da categoria'}
                </h4>
                <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
                  {formData.description || 'Descrição da categoria'}
                </p>
              </div>
              <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                formData.isActive 
                  ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200' 
                  : 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200'
              }`}>
                {formData.isActive ? 'Ativo' : 'Inativo'}
              </span>
            </div>
          </div>
        </div>
      </MainLayout>
    </ProtectedRoute>
  );
};

export default CreateCategoryPage;
