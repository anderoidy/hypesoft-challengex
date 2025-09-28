// hooks/useDeleteCategory.ts

import { useState } from 'react';
import categoryService from '../services/categoryService';

export function useDeleteCategory() {
  const [isDeleting, setIsDeleting] = useState(false);
  const [deleteError, setDeleteError] = useState<string | null>(null);

  const deleteCategory = async (id: string, name: string) => {
    try {
      setIsDeleting(true);
      setDeleteError(null);
      
      console.log('🗑️ Iniciando exclusão da categoria:', name);
      
      // Confirmação dupla
      const confirmed = window.confirm(
        `⚠️ ATENÇÃO!\n\nTem certeza que deseja excluir a categoria "${name}"?\n\n❗ Esta ação não pode ser desfeita!\n❗ Se houver produtos vinculados, a exclusão falhará.`
      );
      
      if (!confirmed) {
        console.log('🚫 Exclusão cancelada pelo usuário');
        return false;
      }
      
      await categoryService.delete(id);
      
      console.log('✅ Categoria excluída com sucesso!');
      
      // Feedback de sucesso
      alert('✅ Categoria excluída com sucesso!');
      
      return true;
      
    } catch (error: any) {
      console.error('❌ Erro na exclusão:', error);
      setDeleteError(error.message);
      
      // Feedback de erro
      alert(`❌ Erro ao excluir categoria:\n\n${error.message}`);
      
      return false;
    } finally {
      setIsDeleting(false);
    }
  };

  return {
    deleteCategory,
    isDeleting,
    deleteError
  };
}
