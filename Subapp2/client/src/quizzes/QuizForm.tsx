import { useState } from 'react';
import type { ComponentProps } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { fetchQuestions } from '../questions/QuestionService';
import type { Quiz } from '../types/quiz';
import { fetchCategories } from '../categories/CategoryService';

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
    const [selectedQuestionIds, setSelectedQuestionIds] = useState<number[]>(
            initialValues?.questionIds ?? [],
        );

        const [selectedCategoryIds, setSelectedCategoryIds] =
            useState<number[] | null>(null);

    const {
        data: questions = [],
        isPending,
        isError,
        error: questionError,
        refetch,
    } = useQuery({
        queryKey: ['questions'],
        // Load all questions independently of the Question list's search filters.
        queryFn: () => fetchQuestions(),
    });

    const {
        data: categories = [],
        isPending: categoriesPending,
        isError: categoriesFailed,
        error: categoryError,
        refetch: refetchCategories,
    } = useQuery({
        queryKey: ['categories'],
        queryFn: fetchCategories,
    });

    // When editing, initially show categories containing the quiz's saved questions.
    const activeCategoryIds =
        selectedCategoryIds ??
        categories
            .filter((category) =>
                questions.some(
                    (question) =>
                        question.categoryId === category.categoryId &&
                        initialValues?.questionIds.includes(question.questionId!),
                ),
            )
            .map((category) => category.categoryId!);


    const visibleQuestions = questions.filter((question) =>
        activeCategoryIds.includes(question.categoryId),
    );

    function handleCategoryChange(categoryId: number, checked: boolean) {
        const nextCategoryIds = checked
            ? [...activeCategoryIds, categoryId]
            : activeCategoryIds.filter((id) => id !== categoryId);

        setSelectedCategoryIds(nextCategoryIds);

        // Removing a category also removes its questions from the quiz selection.
        setSelectedQuestionIds((currentIds) =>
            currentIds.filter((questionId) =>
                questions.some(
                    (question) =>
                        question.questionId === questionId &&
                        nextCategoryIds.includes(question.categoryId),
                ),
            ),
        );
    }


    const handleSubmit: ComponentProps<'form'>['onSubmit'] = async (event) => {
        event.preventDefault();

        const formData = new FormData(event.currentTarget);
        const title = String(formData.get('title') ?? '').trim();
        const description = String(formData.get('description') ?? '').trim();

        // Use React state so questions removed by category changes stay deselected.
        const questionIds = selectedQuestionIds;

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

    if (isPending || categoriesPending) {
        return <p>Loading categories and questions...</p>;
    }

    if (isError || categoriesFailed) {
        return (
            <div className="alert alert-danger" role="alert">
                <p>{questionError?.message ??
                    categoryError?.message ??
                    'Unable to load categories and questions'}</p>
                <button
                    type="button"
                    className="btn btn-outline-danger me-2"
                    onClick={() => {
                        void refetch();
                        void refetchCategories();
                    }}
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
                    <legend className="fs-5">Select categories</legend>

                    {categories.length === 0 && (
                        <p>Create a category and questions before creating a quiz.</p>
                    )}

                    {categories.map((category) => (
                        <div className="form-check" key={category.categoryId}>
                            <input
                                id={`category-${category.categoryId}`}
                                type="checkbox"
                                className="form-check-input"
                                checked={activeCategoryIds.includes(category.categoryId!)}
                                onChange={(event) =>
                                    handleCategoryChange(category.categoryId!, event.target.checked)
                                }
                            />
                            <label
                                htmlFor={`category-${category.categoryId}`}
                                className="form-check-label"
                            >
                                {category.name}
                            </label>
                        </div>
                    ))}
                </fieldset>

                <fieldset className="mb-3">
                    <legend className="fs-5">Select questions</legend>

                    {activeCategoryIds.length === 0 ? (
                        <p>Select a category to see its questions.</p>
                    ) : visibleQuestions.length === 0 ? (
                        <p>No questions exist in the selected categories.</p>
                    ) : (
                        visibleQuestions.map((question) => (
                            <div className="form-check" key={question.questionId}>
                                <input
                                    id={`question-${question.questionId}`}
                                    type="checkbox"
                                    className="form-check-input"
                                    checked={selectedQuestionIds.includes(question.questionId!)}
                                    onChange={(event) => {
                                        const questionId = question.questionId!;

                                        setSelectedQuestionIds((currentIds) =>
                                            event.target.checked
                                                ? [...currentIds, questionId]
                                                : currentIds.filter((id) => id !== questionId),
                                        );
                                    }}
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