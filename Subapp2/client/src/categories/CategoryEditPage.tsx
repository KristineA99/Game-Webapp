import { useQuery, useQueryClient } from '@tanstack/react-query';
import { Link, useNavigate, useParams } from 'react-router-dom';
import CategoryForm from './CategoryForm';
import {
    fetchCategoryById,
    updateCategory,
} from './CategoryService';

export default function CategoryEditPage() {
    const { id } = useParams();
    const categoryId = Number(id);
    const validId = Number.isSafeInteger(categoryId) && categoryId > 0;
    const navigate = useNavigate();
    const queryClient = useQueryClient();

    const {
        data: category,
        isPending,
        isError,
        error,
    } = useQuery({
        queryKey: ['categories', categoryId],
        queryFn: () => fetchCategoryById(categoryId),
        enabled: validId,
        retry: false,
    });

    if (!validId) {
        return (
            <div className="alert alert-danger" role="alert">
                Invalid category ID. <Link to="/admin/categories">Back to categories</Link>
            </div>
        );
    }

    if (isPending) {
        return <p>Loading category...</p>;
    }

    if (isError) {
        return (
            <div className="alert alert-danger" role="alert">
                <p>
                    {error instanceof Error
                        ? error.message
                        : 'Unable to load the category.'}
                </p>
                <Link to="/admin/categories">Back to categories</Link>
            </div>
        );
    }

    async function handleSave(name: string): Promise<void> {
        await updateCategory(categoryId, { categoryId, name });

        // Refresh category data and question data, which also contains category names.
        await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['categories'] }),
        queryClient.invalidateQueries({ queryKey: ['questions'] }),
        ]);

        navigate('/admin/categories');
    }

    return (
        <div>
            <h1>Edit Category</h1>
            <CategoryForm
                key={categoryId}
                initialName={category.name}
                submitLabel="Save Changes"
                onSave={handleSave}
            />
        </div>
    );
}