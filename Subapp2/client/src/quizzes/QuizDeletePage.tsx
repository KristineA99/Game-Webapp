import { useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { deleteQuiz, fetchQuizById } from './QuizService';

export default function QuizDeletePage() {
    const { id } = useParams();
    const quizId = Number(id);
    const validId = Number.isSafeInteger(quizId) && quizId > 0;
    const navigate = useNavigate();
    const queryClient = useQueryClient();
    const [deleteError, setDeleteError] = useState<string | null>(null);
    const [isDeleting, setIsDeleting] = useState(false);

    const {
        data: quiz,
        isPending,
        isError,
        error,
    } = useQuery({
        queryKey: ['quizzes', quizId],
        queryFn: () => fetchQuizById(quizId),
        enabled: validId,
        retry: false,
    });

    if (!validId) {
        return (
            <div className="alert alert-danger" role="alert">
                Invalid quiz ID. <Link to="/admin/quizzes">Back to quizzes</Link>
            </div>
        );
    }

    if (isPending) {
        return <p>Loading quiz...</p>;
    }

    if (isError) {
        return (
            <div className="alert alert-danger" role="alert">
                <p>{error.message}</p>
                <Link to="/admin/quizzes">Back to quizzes</Link>
            </div>
        );
    }

    async function handleDelete(): Promise<void> {
        setDeleteError(null);
        setIsDeleting(true);

        try {
            await deleteQuiz(quizId);

            // Discard the deleted quiz's details and refresh the list.
            queryClient.removeQueries({
                queryKey: ['quizzes', quizId],
                exact: true,
            });
            await queryClient.invalidateQueries({
                queryKey: ['quizzes'],
                exact: true,
            });

            navigate('/admin/quizzes');
        } catch (error) {
            setDeleteError(
                error instanceof Error ? error.message : 'Unable to delete the quiz.',
            );
        } finally {
            setIsDeleting(false);
        }
    }

    return (
        <div>
            <h1>Delete Quiz</h1>
            <p>
                Are you sure you want to delete <strong>{quiz.title}</strong>?
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
                {isDeleting ? 'Deleting...' : 'Delete Quiz'}
            </button>

            {!isDeleting && (
                <Link to="/admin/quizzes" className="btn btn-secondary">
                    Cancel
                </Link>
            )}
        </div>
    );
}