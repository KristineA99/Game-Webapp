// Shape of a question as sent by the API (matches QuestionDto in the backend).
export interface Question {
  questionId?: number; // optional: a new question gets its id from the database
  text: string;
  optionA: string;
  optionB: string;
  optionC: string;
  optionD: string;
  correctOption: number;
  categoryId: number; 
  categoryName?: string | null; 
}