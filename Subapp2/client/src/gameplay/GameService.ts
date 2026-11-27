import { apiGet, apiSend } from '../shared/apiClient';
import type {
    GameQuizSummary,
    GameQuiz,
    GameSubmission,
    GameResult,
} from '../types/game';

const BASE = '/api/game';

export function fetchPlayableQuizzes(): Promise<GameQuizSummary[]> {
    return apiGet<GameQuizSummary[]>(`${BASE}/quizzes`);
}

export function fetchGameQuiz(quizId: number): Promise<GameQuiz> {
    return apiGet<GameQuiz>(`${BASE}/quizzes/${quizId}`);
}

export function submitGameQuiz(
    quizId: number,
    submission: GameSubmission,
): Promise<GameResult> {
    // The shared client obtains an antiforgery token, including for guest players.
    return apiSend<GameResult>(
        'POST',
        `${BASE}/quizzes/${quizId}/submit`,
        submission,
    );
}