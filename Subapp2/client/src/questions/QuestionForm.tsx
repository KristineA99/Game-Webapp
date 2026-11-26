import { useState } from 'react';
import { Alert, Button, Form } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import type { Question } from '../types/question';
import type { Category } from '../types/category';
import * as CategoryService from '../categories/CategoryService';

// The props (input values) the form gets from its parent page.
interface QuestionFormProps {
  onQuestionChanged: (question: Question) => Promise<unknown>; // create or update, decided by the parent
  questionId?: number;          // empty when creating, set when updating
  initialValues?: Question;     // fills in the form when updating
  error?: Error | null;         // error from the API, owned by the parent
  submitting?: boolean;         // true while the request is running
}

export default function QuestionForm({
  onQuestionChanged,
  questionId,
  initialValues,
  error = null,
  submitting = false,
}: QuestionFormProps) {
  const navigate = useNavigate();

  // Local UI state: one useState per form field.
  const [text, setText] = useState(initialValues?.text ?? '');
  const [optionA, setOptionA] = useState(initialValues?.optionA ?? '');
  const [optionB, setOptionB] = useState(initialValues?.optionB ?? '');
  const [optionC, setOptionC] = useState(initialValues?.optionC ?? '');
  const [optionD, setOptionD] = useState(initialValues?.optionD ?? '');
  const [correctOption, setCorrectOption] = useState(initialValues?.correctOption ?? 0);
  const [categoryId, setCategoryId] = useState(initialValues?.categoryId ?? 0);

  // Fetch the categories for the dropdown.
  const { data: categories = [], error: categoryError } = useQuery<Category[], Error>({
    queryKey: ['categories'],
    queryFn: CategoryService.fetchCategories,
  });

  const onCancel = () => navigate(-1); // go back to the previous page

  const handleSubmit = async (event: React.SyntheticEvent<HTMLFormElement>) => {
    event.preventDefault(); // stop the browser from reloading the page

    const question: Question = {
      questionId,
      text,
      optionA,
      optionB,
      optionC,
      optionD,
      correctOption,
      categoryId,
    };

    try {
      await onQuestionChanged(question);
    } catch {
      // The error is already shown through the error prop from useMutation,
      // so we only catch it here to avoid an unhandled promise rejection.
    }
  };

  return (
    <Form onSubmit={handleSubmit}>
      <Form.Group className="mb-3" controlId="questionText">
        <Form.Label>Question</Form.Label>
        <Form.Control
          as="textarea"
          rows={2}
          value={text}
          onChange={e => setText(e.target.value)}
          required
          minLength={5}
          maxLength={300}
          placeholder="Write the question"
        />
      </Form.Group>

      <Form.Group className="mb-3" controlId="questionCategory">
        <Form.Label>Category</Form.Label>
        <Form.Select
          value={categoryId}
          onChange={e => setCategoryId(Number(e.target.value))}
          required
        >
          <option value="">Choose a category</option>
          {categories.map(category => (
            <option key={category.categoryId} value={category.categoryId}>
              {category.name}
            </option>
          ))}
        </Form.Select>
        {categoryError && (
          <Form.Text className="text-danger">{categoryError.message}</Form.Text>
        )}
      </Form.Group>

      {/* The four answer options */}
      <Form.Group className="mb-3" controlId="optionA">
        <Form.Label>Option A</Form.Label>
        <Form.Control value={optionA} onChange={e => setOptionA(e.target.value)} required maxLength={100} />
      </Form.Group>
      <Form.Group className="mb-3" controlId="optionB">
        <Form.Label>Option B</Form.Label>
        <Form.Control value={optionB} onChange={e => setOptionB(e.target.value)} required maxLength={100} />
      </Form.Group>
      <Form.Group className="mb-3" controlId="optionC">
        <Form.Label>Option C</Form.Label>
        <Form.Control value={optionC} onChange={e => setOptionC(e.target.value)} required maxLength={100} />
      </Form.Group>
      <Form.Group className="mb-3" controlId="optionD">
        <Form.Label>Option D</Form.Label>
        <Form.Control value={optionD} onChange={e => setOptionD(e.target.value)} required maxLength={100} />
      </Form.Group>

      <Form.Group className="mb-3" controlId="correctOption">
        <Form.Label>Correct answer</Form.Label>
        <Form.Select
          value={correctOption}
          onChange={e => setCorrectOption(Number(e.target.value))}
          required
        >
          {/* 1 = A, 2 = B, 3 = C, 4 = D, the same numbers the API uses */}
          <option value="">Choose the correct answer</option>
          <option value={1}>A</option>
          <option value={2}>B</option>
          <option value={3}>C</option>
          <option value={4}>D</option>
        </Form.Select>
      </Form.Group>

      {/* Error from the API, for example a validation message */}
      {error && <Alert variant="danger">{error.message}</Alert>}

      <Button variant="primary" type="submit" disabled={submitting}>
        {submitting ? 'Saving...' : questionId ? 'Update Question' : 'Create Question'}
      </Button>
      <Button variant="secondary" onClick={onCancel} className="ms-2" disabled={submitting}>
        Cancel
      </Button>
    </Form>
  );
}