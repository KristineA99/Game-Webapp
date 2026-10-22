import { useQuery } from '@tanstack/react-query';
import { Table, Button } from 'react-bootstrap';
import type { Question } from '../types/question';
import * as QuestionService from './QuestionService';

// Converts the stored number (1-4) to the letter shown to the user
const toOptionLetter = (option: number): string => ['A', 'B', 'C', 'D'][option - 1] ?? '?';

export default function QuestionListPage() {
  // useQuery fetches the data and keeps track of loading and error states
  const { data: questions = [], isLoading, error, refetch } = useQuery<Question[], Error>({
    queryKey: ['questions'],
    queryFn: QuestionService.fetchQuestions,
  });

  return (
    <div>
      <h1>Questions</h1>

      <Button onClick={() => refetch()} className="mb-3" disabled={isLoading}>
        {isLoading ? 'Loading...' : 'Refresh Questions'}
      </Button>

      {/* Conditional rendering (&&): only shown when an error exists */}
      {error && <p style={{ color: 'red' }}>{error.message}</p>}

      {/* Conditional rendering (ternary): message when the list is empty, otherwise the table */}
      {!isLoading && questions.length === 0 ? (
        <p>No questions found.</p>
      ) : (
        <Table striped bordered hover>
          <thead>
            <tr>
              <th>ID</th>
              <th>Question</th>
              <th>A</th>
              <th>B</th>
              <th>C</th>
              <th>D</th>
              <th>Correct</th>
            </tr>
          </thead>
          <tbody>
            {questions.map(question => (
              <tr key={question.questionId}>
                <td>{question.questionId}</td>
                <td>{question.text}</td>
                <td>{question.optionA}</td>
                <td>{question.optionB}</td>
                <td>{question.optionC}</td>
                <td>{question.optionD}</td>
                <td>{toOptionLetter(question.correctOption)}</td>
              </tr>
            ))}
          </tbody>
        </Table>
      )}
    </div>
  );
}