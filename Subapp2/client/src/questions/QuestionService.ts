import type { Question } from '../types/question';

const API_URL = import.meta.env.VITE_API_URL;

// Shared helper: throws an error for failed requests, otherwise returns the parsed JSON.
async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    throw new Error(`Request failed (status ${response.status})`);
  }
  return response.json();
}

// GET all questions
export async function fetchQuestions(): Promise<Question[]> {
  const response = await fetch(`${API_URL}/api/questionapi/questionlist`);
  return handleResponse<Question[]>(response);
}