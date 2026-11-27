import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Link, useParams } from 'react-router-dom';
import GameQuestion from './GameQuestion';
import { fetchGameQuiz, submitGameQuiz } from './GameService';
import type { GameAnswer, GameResult } from '../types/game';

export default function GamePage() {
    const { quizId } = useParams();
    const id = Number(quizId);
    const validId = Number.isSafeInteger(id) && id > 0;

    const [questionIndex, setQuestionIndex] = useState(0);
    const [answers, setAnswers] = useState<GameAnswer[]>([]);
    const [result, setResult] = useState<GameResult | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);

    const {
        data: quiz,
        isPending,
        isError,
        error: loadError,
    } = useQuery({
        queryKey: ['game', 'quiz', id],
        queryFn: () => fetchGameQuiz(id),
        enabled: validId,
        retry: false,
        // Keep the displayed question set stable while the player answers.
        staleTime: Infinity,
        refetchOnWindowFocus: false,
    });

    if (!validId) {
        return (
            <div className="alert alert-danger" role="alert">
                Invalid quiz ID. <Link to="/play">Choose a quiz</Link>
            </div>
        );
    }

    if (isPending) {
        return <p>Loading quiz...</p>;
    }

    if (isError) {
        return (
            <div className="alert alert-danger" role="alert">
                <p>{loadError.message}</p>
                <Link to="/play">Choose another quiz</Link>
            </div>
        );
    }

    if (result) {
        return (
            <div>
                <h1>Quiz Complete</h1>
                <h2 className="h4">{result.quizTitle}</h2>
                <p className="fs-4">
                    Your score: {result.score} / {result.totalQuestions}
                </p>
                <Link to="/play" className="btn btn-primary">
                    Choose Another Quiz
                </Link>
            </div>
        );
    }

    const question = quiz.questions[questionIndex];

    if (!question) {
        return (
            <div className="alert alert-warning" role="alert">
                No question is available. <Link to="/play">Choose another quiz</Link>
            </div>
        );
    }

    const selectedOption = answers.find(
        (answer) => answer.questionId === question.questionId,
    )?.selectedOption;

    function handleSelect(option: number) {
        setError(null);

        // Replace this question's answer rather than adding duplicate entries.
        setAnswers((current) => [
            ...current.filter((answer) => answer.questionId !== question.questionId),
            { questionId: question.questionId, selectedOption: option },
        ]);
    }

    async function handleContinue() {
        if (!quiz || selectedOption === undefined || isSubmitting) {
            return;
        }

        if (questionIndex < quiz.questions.length - 1) {
            setQuestionIndex((current) => current + 1);
            return;
        }

        setError(null);
        setIsSubmitting(true);

        try {
            // Only the server validates the complete submission and calculates a score.
            const response = await submitGameQuiz(id, { answers });
            setResult(response);
        } catch (error) {
            setError(
                error instanceof Error
                    ? error.message
                    : 'Unable to submit your answers. Please try again.',
            );
        } finally {
            setIsSubmitting(false);
        }
    }

    return (
        <div>
            <h1>{quiz.title}</h1>

            <GameQuestion
                question={question}
                questionNumber={questionIndex + 1}
                totalQuestions={quiz.questions.length}
                selectedOption={selectedOption}
                isSubmitting={isSubmitting}
                error={error}
                onSelect={handleSelect}
                onContinue={() => void handleContinue()}
            />

            <Link to="/play" className="d-inline-block mt-3">
                Leave Quiz
            </Link>
        </div>
    );
}