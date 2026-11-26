import { useState } from 'react';
import type { ComponentProps } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { fetchQuestions } from '../questions/QuestionService';
import type { Quiz } from '../types/quiz';

interface QuizFormProps {
    initialValues?: Quiz;
    submitLabel: string;
    onSave: (quiz: Quiz) => Promise<void>;
}

export default function QuizForm({
    initialValues,
    submitLabel,
    onSave,
}: QuizFormProps) {
    const [error, setError] = useState<string | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);

    const {
        data: questions = [],
        isPending,
        isError,
        error: questionError,
        refetch,
    } = useQuery({
        queryKey: ['questions'],
        queryFn: fetchQuestions,
    });

    const handleSubmit: ComponentProps<'form'>['onSubmit'] = async (event) => {
        event.preventDefault();

        const formData = new FormData(event.currentTarget);
        const title = String(formData.get('title') ?? '').trim();
        const description = String(formData.get('description') ?? '').trim();

        // Checked boxes share a name, so getAll collects every selected question ID.
        const questionIds = formData.getAll('questionIds').map(Number);

        setError(null);

        if (title.length < 3 || title.length > 100) {
            setError('The title must be between 3 and 100 characters.');
            return;
        }

        if (description.length > 500) {
            setError('The description can be at most 500 characters.');
            return;
        }

        if (questionIds.length === 0) {
            setError('Select at least one question.');
            return;
        }

        setIsSubmitting(true);

        try {
            // The parent page supplies either the create or update operation.
            await onSave({ title, description, questionIds });
        } catch (error) {
            setError(
                error instanceof Error ? error.message : 'Unable to save the quiz.',
            );
        } finally {
            setIsSubmitting(false);
        }
    };

    if (isPending) {
        return <p>Loading available questions...</p>;
    }

    if (isError) {
        return (
            <div className="alert alert-danger" role="alert">
                <p>{questionError.message}</p>
                <button
                    type="button"
                    className="btn btn-outline-danger me-2"
                    onClick={() => void refetch()}
                >
                    Try again
                </button>
                <Link to="/admin/quizzes">Back to quizzes</Link>
            </div>
        );
    }

    return (
        <form onSubmit={handleSubmit} noValidate>

            <fieldset disabled={isSubmitting}>
                <div className="mb-3">
                    <label htmlFor="title" className="form-label">
                        Title
                    </label>
                    <input
                        id="title"
                        name="title"
                        className="form-control"
                        defaultValue={initialValues?.title ?? ''}
                        required
                        minLength={3}
                        maxLength={100}
                    />
                </div>

                <div className="mb-3">
                    <label htmlFor="description" className="form-label">
                        Description (optional)
                    </label>
                    <textarea
                        id="description"
                        name="description"
                        className="form-control"
                        defaultValue={initialValues?.description ?? ''}
                        maxLength={500}
                        rows={3}
                    />
                </div>

                <fieldset className="mb-3">
                    <legend className="fs-5">Select questions</legend>

                    {questions.length === 0 ? (
                        <p>Create a question before creating a quiz.</p>
                    ) : (
                        questions.map((question) => (
                            <div className="form-check" key={question.questionId}>
                                <input
                                    id={`question-${question.questionId}`}
                                    name="questionIds"
                                    type="checkbox"
                                    className="form-check-input"
                                    value={question.questionId}
                                    defaultChecked={initialValues?.questionIds.includes(
                                        question.questionId!,
                                    ) ?? false}
                                />
                                <label
                                    htmlFor={`question-${question.questionId}`}
                                    className="form-check-label"
                                >
                                    {question.text}
                                </label>
                            </div>
                        ))
                    )}
                </fieldset>

                {error && (
                    <div className="alert alert-danger" role="alert">
                        {error}
                    </div>
                )}

                <button
                    type="submit"
                    className="btn btn-primary"
                    disabled={questions.length === 0}
                >
                    {isSubmitting ? 'Saving...' : submitLabel}
                </button>
            </fieldset>

            {!isSubmitting && (
                <Link to="/admin/quizzes" className="btn btn-secondary mt-2">
                    Cancel
                </Link>
            )}
        </form>
    );
}