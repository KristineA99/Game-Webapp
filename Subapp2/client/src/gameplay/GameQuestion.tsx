import type { GameQuestion as GameQuestionData } from '../types/game';

interface GameQuestionProps {
    question: GameQuestionData;
    questionNumber: number;
    totalQuestions: number;
    selectedOption?: number;
    isSubmitting: boolean;
    error: string | null;
    onSelect: (option: number) => void;
    onContinue: () => void;
}

export default function GameQuestion({
    question,
    questionNumber,
    totalQuestions,
    selectedOption,
    isSubmitting,
    error,
    onSelect,
    onContinue,
}: GameQuestionProps) {
    const options = [
        question.optionA,
        question.optionB,
        question.optionC,
        question.optionD,
    ];

    const isLastQuestion = questionNumber === totalQuestions;

    return (
        <div>
            <p>Question {questionNumber} of {totalQuestions}</p>
            <h2 className="h4">{question.text}</h2>

            <fieldset disabled={isSubmitting} className="mb-3">
                <legend className="fs-6">Choose one answer</legend>

                {options.map((text, index) => {
                    // Match the API's option numbering: 1 = A through 4 = D.
                    const option = index + 1;
                    const inputId = `answer-${question.questionId}-${option}`;

                    return (
                        <div className="form-check mb-2" key={option}>
                            <input
                                id={inputId}
                                name={`question-${question.questionId}`}
                                type="radio"
                                className="form-check-input"
                                checked={selectedOption === option}
                                onChange={() => onSelect(option)}
                            />
                            <label htmlFor={inputId} className="form-check-label">
                                {text}
                            </label>
                        </div>
                    );
                })}
            </fieldset>

            {error && (
                <div className="alert alert-danger" role="alert">
                    {error}
                </div>
            )}

            <button
                type="button"
                className="btn btn-primary"
                disabled={selectedOption === undefined || isSubmitting}
                onClick={onContinue}>
                {isSubmitting
                    ? 'Submitting...'
                    : isLastQuestion
                        ? 'Submit Quiz'
                        : 'Continue'}
            </button>
        </div>
    );
}