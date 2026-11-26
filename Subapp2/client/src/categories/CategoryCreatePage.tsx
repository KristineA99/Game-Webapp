import { useNavigate } from 'react-router-dom';
import { useQueryClient } from '@tanstack/react-query';
import CategoryForm from './CategoryForm';
import { createCategory } from './CategoryService';

export default function CategoryCreatePage() {
    const navigate = useNavigate();
    const queryClient = useQueryClient();

    async function handleSave(name: string): Promise<void> {
        await createCategory({ name });

        // Refresh the list before returning so it includes the new category.
        await queryClient.invalidateQueries({ queryKey: ['categories'] });
        navigate('/admin/categories');
    }

    return (
        <div>
            <h1>Create Category</h1>
            <CategoryForm
                submitLabel="Create Category"
                onSave={handleSave}
            />
        </div>
    );
}