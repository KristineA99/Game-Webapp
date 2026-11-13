// Shape of a quiz as sent by the API (matches QuizDto in the backend).
export interface Quiz {
  quizId?: number; // optional: a new quiz gets its id from the database
  title: string;
  description?: string | null;
  questionIds: number[]; s
}
