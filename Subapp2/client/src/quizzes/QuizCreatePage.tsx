import { useNavigate } from 'react-router-dom';
import { useQueryClient } from '@tanstack/react-query';
import QuizForm from './QuizForm';
import { createQuiz } from './QuizService';
import type { Quiz } from '../types/quiz';

export default function QuizCreatePage() {
    const navigate = useNavigate();
    const queryClient = useQueryClient();

    async function handleSave(quiz: Quiz): Promise<void> {
        await createQuiz(quiz);

        // Refresh cached quiz data so the list includes the new quiz.
        await queryClient.invalidateQueries({ queryKey: ['quizzes'] });
        navigate('/admin/quizzes');
    }

    return (
        <div>
            <h1>Create Quiz</h1>
            <QuizForm submitLabel="Create Quiz" onSave={handleSave} />
        </div>
    );
}