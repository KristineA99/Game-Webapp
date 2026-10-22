// Shape of a question as sent by the API (matches QuestionDto in the backend).
// .NET converts property names to camelCase in JSON, e.g. QuestionId -> questionId.
export interface Question {
  questionId: number;
  text: string;
  optionA: string;
  optionB: string;
  optionC: string;
  optionD: string;
  correctOption: number;
}