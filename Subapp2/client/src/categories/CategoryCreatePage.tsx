import { useState } from 'react';
import type { ComponentProps } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useQueryClient } from '@tanstack/react-query';
import { createCategory } from './CategoryService';

export default function CategoryCreatePage() {
    const navigate = useNavigate();
    const queryClient = useQueryClient();
    const [error, setError] = useState<string | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);

    const handleSubmit: ComponentProps<'form'>['onSubmit'] = async (event) => {
        event.preventDefault();

        const formData = new FormData(event.currentTarget);
        const name = String(formData.get('name') ?? '').trim();

        setError(null);

        // Validate the trimmed value so spaces cannot satisfy the name requirement.
        if (name.length < 2 || name.length > 100) {
            setError('The name must be between 2 and 100 characters.');
            return;
        }

        setIsSubmitting(true);

        try {
            await createCategory({ name });

            // Refresh the cached list so the newly created category appears.
            await queryClient.invalidateQueries({ queryKey: ['categories'] });
            navigate('/admin/categories');
        } catch (error) {
            setError(
                error instanceof Error
                    ? error.message
                    : 'Unable to create the category.',
            );
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <div>
            <h1>Create Category</h1>

            {error && (
                <div className="alert alert-danger" role="alert">
                    {error}
                </div>
            )}

            <form onSubmit={handleSubmit}>
                <div className="mb-3">
                    <label htmlFor="name" className="form-label">
                        Category name
                    </label>
                    <input
                        id="name"
                        name="name"
                        type="text"
                        className="form-control"
                        required
                        minLength={2}
                        maxLength={100}
                        disabled={isSubmitting}
                    />
                </div>

                <button
                    type="submit"
                    className="btn btn-primary me-2"
                    disabled={isSubmitting}
                >
                    {isSubmitting ? 'Saving...' : 'Create Category'}
                </button>

                {!isSubmitting && (
                    <Link to="/admin/categories" className="btn btn-secondary">
                        Cancel
                    </Link>
                )}
            </form>
        </div>
    );
}