import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useNavigate, useParams } from 'react-router-dom';
import { Alert } from 'react-bootstrap';
import type { Question } from '../types/question';
import QuestionForm from './QuestionForm';
import * as QuestionService from './QuestionService';

export default function QuestionUpdatePage() {
  // Reads the id from the URL, for example /admin/questions/update/5
  const { questionId } = useParams<{ questionId: string }>();
  const id = Number(questionId);
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  // 1) Fetch the question that should be edited.
  const { data: question, isLoading, error } = useQuery<Question, Error>({
    queryKey: ['questions', id],
    queryFn: () => QuestionService.fetchQuestionById(id),
    enabled: !!id, // wait until the id is known
  });

  // 2) Send the changes to the API.
  const updateMutation = useMutation({
    mutationFn: (changed: Question) => QuestionService.updateQuestion(id, changed),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['questions'] });
      navigate('/admin/questions');
    },
  });

  if (isLoading) return <p>Loading...</p>;
  if (error) return <Alert variant="danger">{error.message}</Alert>;
  if (!question) return <p>Question not found.</p>;

  return (
    <div>
      <h2>Update Question</h2>
      <QuestionForm
        onQuestionChanged={updateMutation.mutateAsync}
        questionId={id}
        initialValues={question}
        error={updateMutation.error}
        submitting={updateMutation.isPending}
      />
    </div>
  );
}