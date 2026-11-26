import type { Quiz } from '../types/quiz';
import { apiGet, apiSend } from '../shared/apiClient';

const BASE = '/api/quizapi';

export function fetchQuizzes(): Promise<Quiz[]> {
  return apiGet<Quiz[]>(`${BASE}/quizlist`);
}

export function fetchQuizById(id: number): Promise<Quiz> {
  return apiGet<Quiz>(`${BASE}/${id}`);
}

export function createQuiz(quiz: Quiz): Promise<Quiz> {
  return apiSend<Quiz>('POST', `${BASE}/create`, quiz);
}

export function updateQuiz(id: number, quiz: Quiz): Promise<Quiz> {
  return apiSend<Quiz>('PUT', `${BASE}/update/${id}`, quiz);
}

export function deleteQuiz(id: number): Promise<void> {
  return apiSend<void>('DELETE', `${BASE}/delete/${id}`);
}