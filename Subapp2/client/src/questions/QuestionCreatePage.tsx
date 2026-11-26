import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import QuestionForm from './QuestionForm';
import * as QuestionService from './QuestionService';

export default function QuestionCreatePage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  // useMutation is used for requests that CHANGE data (POST, PUT, DELETE).
  const createMutation = useMutation({
    mutationFn: QuestionService.createQuestion,
    onSuccess: () => {
      // Mark the question list as outdated so it is fetched again.
      queryClient.invalidateQueries({ queryKey: ['questions'] });
      navigate('/admin/questions');
    },
  });

  return (
    <div>
      <h2>Create New Question</h2>
      <QuestionForm
        onQuestionChanged={createMutation.mutateAsync}
        error={createMutation.error}
        submitting={createMutation.isPending}
      />
    </div>
  );
}