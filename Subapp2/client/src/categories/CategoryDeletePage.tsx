import { useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { Link, useNavigate, useParams } from 'react-router-dom';
import {
    deleteCategory,
    fetchCategoryById,
} from './CategoryService';

export default function CategoryDeletePage() {
    const { id } = useParams();
    const categoryId = Number(id);
    const validId = Number.isSafeInteger(categoryId) && categoryId > 0;
    const navigate = useNavigate();
    const queryClient = useQueryClient();
    const [deleteError, setDeleteError] = useState<string | null>(null);
    const [isDeleting, setIsDeleting] = useState(false);

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

    async function handleDelete(): Promise<void> {
        setDeleteError(null);
        setIsDeleting(true);

        try {
            await deleteCategory(categoryId);

            // Remove the deleted record from the cache before refreshing the list.
            queryClient.removeQueries({
                queryKey: ['categories', categoryId],
                exact: true,
            });
            await queryClient.invalidateQueries({
                queryKey: ['categories'],
                exact: true,
            });

            navigate('/admin/categories');
        } catch (error) {
            // The API also rejects deletion when the category contains questions.
            setDeleteError(
                error instanceof Error
                    ? error.message
                    : 'Unable to delete the category.',
            );
        } finally {
            setIsDeleting(false);
        }
    }

    return (
        <div>
            <h1>Delete Category</h1>
            <p>
                Are you sure you want to delete <strong>{category.name}</strong>?
                This cannot be undone.
            </p>

            {deleteError && (
                <div className="alert alert-danger" role="alert">
                    {deleteError}
                </div>
            )}

            <button
                type="button"
                className="btn btn-danger me-2"
                disabled={isDeleting}
                onClick={() => void handleDelete()}
            >
                {isDeleting ? 'Deleting...' : 'Delete Category'}
            </button>

            {!isDeleting && (
                <Link to="/admin/categories" className="btn btn-secondary">
                    Cancel
                </Link>
            )}
        </div>
    );
}