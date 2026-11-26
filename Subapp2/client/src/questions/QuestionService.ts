import type { Question, QuestionFilter } from '../types/question';
import { apiGet, apiSend } from '../shared/apiClient';

const BASE = '/api/questionapi';

// GET questions. Without a filter all questions are returned,
export function fetchQuestions(filter: QuestionFilter = {}): Promise<Question[]> {
  const params = new URLSearchParams();
  if (filter.search?.trim()) params.set('search', filter.search.trim());
  if (filter.categoryId) params.set('categoryId', String(filter.categoryId));

  const query = params.toString();
  return apiGet<Question[]>(`${BASE}/questionlist${query ? `?${query}` : ''}`);
}

// GET one question
export function fetchQuestionById(id: number): Promise<Question> {
  return apiGet<Question>(`${BASE}/${id}`);
}

// POST a new question. The API returns it with its new id.
export function createQuestion(question: Question): Promise<Question> {
  return apiSend<Question>('POST', `${BASE}/create`, question);
}

// PUT changes to an existing question
export function updateQuestion(id: number, question: Question): Promise<Question> {
  return apiSend<Question>('PUT', `${BASE}/update/${id}`, question);
}

// DELETE a question. The API returns 204 No Content.
export function deleteQuestion(id: number): Promise<void> {
  return apiSend<void>('DELETE', `${BASE}/delete/${id}`);
}