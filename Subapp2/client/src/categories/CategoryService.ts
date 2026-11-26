import type { Category } from '../types/category';
import { apiGet, apiSend } from '../shared/apiClient';

const BASE = '/api/categoryapi';

export function fetchCategories(): Promise<Category[]> {
  return apiGet<Category[]>(`${BASE}/categorylist`);
}

export function fetchCategoryById(id: number): Promise<Category> {
  return apiGet<Category>(`${BASE}/${id}`);
}

export function createCategory(category: Category): Promise<Category> {
  return apiSend<Category>('POST', `${BASE}/create`, category);
}

export function updateCategory(id: number, category: Category): Promise<Category> {
  return apiSend<Category>('PUT', `${BASE}/update/${id}`, category);
}

export function deleteCategory(id: number): Promise<void> {
  return apiSend<void>('DELETE', `${BASE}/delete/${id}`);
}