import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Alert, Table, Button } from 'react-bootstrap';
import { Link } from 'react-router-dom';
import type { Question } from '../types/question';
import * as QuestionService from './QuestionService';

// Converts the stored number (1-4) to the letter shown to the user
const toOptionLetter = (option: number): string => ['A', 'B', 'C', 'D'][option - 1] ?? '?';

export default function QuestionListPage() {
  const queryClient = useQueryClient(); 

  // useQuery fetches the data and keeps track of loading and error states
  const { data: questions = [], isLoading, error, refetch } = useQuery<Question[], Error>({
    queryKey: ['questions'],
    queryFn: QuestionService.fetchQuestions,
  });

   // useMutation for delete. On success the list is fetched again.
  const deleteMutation = useMutation({
    mutationFn: QuestionService.deleteQuestion,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['questions'] }),
  });

  const handleQuestionDeleted = (question: Question) => {
    // Ask the user to confirm before deleting.
    const confirmed = window.confirm(`Are you sure you want to delete the question "${question.text}"?`);
    if (confirmed && question.questionId) {
      deleteMutation.mutate(question.questionId);
    }
  };

  return (
    <div>
      <h1>Questions</h1>

      <div className="mb-3 d-flex gap-2">
        <Link to="/admin/questions/create" className="btn btn-success">
          Add New Question
        </Link>
      
      <Button onClick={() => refetch()} disabled={isLoading}>
        {isLoading ? 'Loading...' : 'Refresh Questions'}
      </Button>
      </div>

      {/* Conditional rendering (&&): only shown when an error exists */}
      {error && <Alert variant="danger">{error.message}</Alert>}
      {deleteMutation.error && <Alert variant="danger">{deleteMutation.error.message}</Alert>}

      {/* Conditional rendering (ternary): message when the list is empty, otherwise the table */}
      {!isLoading && questions.length === 0 ? (
        <p>No questions found.</p>
      ) : (
        <Table striped bordered hover>
          <thead>
            <tr>
              <th>ID</th>
              <th>Question</th>
              <th>Category</th>
              <th>A</th>
              <th>B</th>
              <th>C</th>
              <th>D</th>
              <th>Correct</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {questions.map(question => (
              <tr key={question.questionId}>
                <td>{question.questionId}</td>
                <td>{question.text}</td>
                <td>{question.categoryName ?? '-'}</td>
                <td>{question.optionA}</td>
                <td>{question.optionB}</td>
                <td>{question.optionC}</td>
                <td>{question.optionD}</td>
                <td>{toOptionLetter(question.correctOption)}</td>
                <td className="text-nowrap">
                  <Link
                    to={`/admin/questions/update/${question.questionId}`}
                    className="btn btn-primary btn-sm me-2"
                  >
                    Edit
                  </Link>
                  <Button
                    variant="danger"
                    size="sm"
                    onClick={() => handleQuestionDeleted(question)}
                    disabled={deleteMutation.isPending}
                  >
                    Delete
                  </Button>
                </td>
              </tr>
            ))}
          </tbody>
        </Table>
      )}
    </div>
  );
}