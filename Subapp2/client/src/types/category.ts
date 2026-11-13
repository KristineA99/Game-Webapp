// Shape of a category as sent by the API (matches CategoryDto in the backend).
export interface Category {
  categoryId?: number; // optional: a new category gets its id from the database
  name: string;
}
