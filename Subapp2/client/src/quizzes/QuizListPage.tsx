import { useQuery } from '@tanstack/react-query';
import { fetchQuizzes } from './QuizService';
import { Link } from 'react-router-dom';

export default function QuizListPage() {
    const {
        data: quizzes = [],
        isPending,
        isError,
        error,
        refetch,
    } = useQuery({
        queryKey: ['quizzes'],
        queryFn: fetchQuizzes,
    });

    if (isPending) {
        return <p>Loading quizzes...</p>;
    }

    if (isError) {
        return (
            <div className="alert alert-danger" role="alert">
                <p>
                    {error instanceof Error
                        ? error.message
                        : 'Unable to load quizzes.'}
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
            <h1>Manage Quizzes</h1>

            <Link to="/admin/quizzes/create" className="btn btn-primary mb-3">
                Create Quiz
            </Link>

            {quizzes.length === 0 ? (
                <p>No quizzes have been created yet.</p>
            ) : (
                <table className="table table-striped">
                    <thead>
                        <tr>
                            <th scope="col">Title</th>
                            <th scope="col">Description</th>
                            <th scope="col">Questions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {quizzes.map((quiz) => (
                            <tr key={quiz.quizId}>
                                <td>{quiz.title}</td>
                                <td>{quiz.description || 'No description'}</td>
                                <td>{quiz.questionIds.length}</td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}
        </div>
    );
}