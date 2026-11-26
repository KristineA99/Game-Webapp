import { useQuery } from '@tanstack/react-query';
import { fetchCategories } from './CategoryService';

export default function CategoryListPage() {
    const {
        data: categories = [],
        isPending,
        isError,
        error,
        refetch,
    } = useQuery({
        queryKey: ['categories'],
        queryFn: fetchCategories,
    });

    if (isPending) {
        return <p>Loading categories...</p>;
    }

    if (isError) {
        return (
            <div className="alert alert-danger" role="alert">
                <p>
                    {error instanceof Error
                        ? error.message
                        : 'Unable to load categories.'}
                </p>
                <button
                    type="button"
                    className="btn btn-outline-danger"
                    onClick={() => void refetch()}
                >
                    Try again
                </button>
            </div>
        );
    }

    return (
        <div>
            <h1>Manage Categories</h1>

            {categories.length === 0 ? (
                <p>No categories have been created yet.</p>
            ) : (
                <table className="table table-striped">
                    <thead>
                        <tr>
                            <th scope="col">Name</th>
                        </tr>
                    </thead>
                    <tbody>
                        {categories.map((category) => (
                            <tr key={category.categoryId}>
                                <td>{category.name}</td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}
        </div>
    );
}