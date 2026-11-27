import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { fetchPlayableQuizzes } from './GameService';

export default function QuizSelectionPage() {
    const {
        data: quizzes = [],
        isPending,
        isError,
        error,
        refetch,
    } = useQuery({
        // Keep public gameplay data separate from Admin management queries.
        queryKey: ['game', 'quizzes'],
        queryFn: fetchPlayableQuizzes,
    });

    if (isPending) {
        return <p>Loading quizzes...</p>;
    }

    if (isError) {
        return (
            <div className="alert alert-danger" role="alert">
                <p>{error.message}</p>
                <button
                    type="button"
                    className="btn btn-outline-danger"
                    onClick={() => void refetch()}>
                    Try again
                </button>
            </div>
        );
    }

    return (
        <div>
            <h1>Choose a Quiz</h1>

            {quizzes.length === 0 ? (
                <p>No playable quizzes are available yet.</p>
            ) : (
                <div className="row g-3">
                    {quizzes.map((quiz) => (
                        <div className="col-md-4" key={quiz.quizId}>
                            <div className="card h-100">
                                <div className="card-body">
                                    <h2 className="h5">{quiz.title}</h2>
                                    {quiz.description && <p>{quiz.description}</p>}
                                    <p>{quiz.questionCount} questions</p>
                                    <Link
                                        to={`/play/${quiz.quizId}`}
                                        className="btn btn-primary">
                                        Play Quiz
                                    </Link>
                                </div>
                            </div>
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
}