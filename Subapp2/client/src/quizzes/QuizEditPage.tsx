import { useQuery, useQueryClient } from '@tanstack/react-query';
import { Link, useNavigate, useParams } from 'react-router-dom';
import QuizForm from './QuizForm';
import { fetchQuizById, updateQuiz } from './QuizService';
import type { Quiz } from '../types/quiz';

export default function QuizEditPage() {
    const { id } = useParams();
    const quizId = Number(id);
    const validId = Number.isSafeInteger(quizId) && quizId > 0;
    const navigate = useNavigate();
    const queryClient = useQueryClient();

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

    async function handleSave(values: Quiz): Promise<void> {
        await updateQuiz(quizId, { ...values, quizId });

        // Refresh the list and cached details after changing the quiz.
        await queryClient.invalidateQueries({ queryKey: ['quizzes'] });
        navigate('/admin/quizzes');
    }

    return (
        <div>
            <h1>Edit Quiz</h1>
            <QuizForm
                key={quizId}
                initialValues={quiz}
                submitLabel="Save Changes"
                onSave={handleSave}
            />
        </div>
    );
}