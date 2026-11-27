export interface GameQuizSummary {
    quizId: number;
    title: string;
    description: string | null;
    questionCount: number;
}

// Correct answers are never included in the gameplay question response.
export interface GameQuestion {
    questionId: number;
    text: string;
    optionA: string;
    optionB: string;
    optionC: string;
    optionD: string;
}

export interface GameQuiz {
    quizId: number;
    title: string;
    description: string | null;
    questions: GameQuestion[];
}

export interface GameAnswer {
    questionId: number;
    selectedOption: number;
}

export interface GameSubmission {
    answers: GameAnswer[];
}

// Both score values come from the server, not from frontend calculations.
export interface GameResult {
    quizId: number;
    quizTitle: string;
    score: number;
    totalQuestions: number;
}