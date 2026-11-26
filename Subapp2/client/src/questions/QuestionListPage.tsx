import { useState } from 'react';
import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Alert, Table, Button, Form, Row, Col } from 'react-bootstrap';
import { Link } from 'react-router-dom';
import type { Question, QuestionFilter } from '../types/question';
import type { Category } from '../types/category';
import * as QuestionService from './QuestionService';
import * as CategoryService from '../categories/CategoryService';

// Converts the stored number (1-4) to the letter shown to the user
const toOptionLetter = (option: number): string => ['A', 'B', 'C', 'D'][option - 1] ?? '?';

export default function QuestionListPage() {
  const queryClient = useQueryClient(); 

  // Filter values chosen by the user (local UI state).
  const [search, setSearch] = useState('');
  const [categoryId, setCategoryId] = useState(0); // 0 = all categories

  const filter: QuestionFilter = { search, categoryId: categoryId || undefined };
  const isFiltering = search.trim() !== '' || categoryId !== 0;
  
  // The filter is part of the queryKey, so a new request is sent when it changes.
  const { data: questions = [], isLoading, error} = useQuery<Question[], Error>({
    queryKey: ['questions', filter],
    queryFn: () => QuestionService.fetchQuestions(filter),
    placeholderData: keepPreviousData, // keep showing the old list while the new one loads
  });

  // Categories for the filter dropdown (same cache key as the question form).
  const { data: categories = [] } = useQuery< Category[], Error>({
    queryKey: ['categories'],
    queryFn: CategoryService.fetchCategories,
  });

  const resetFilters = () => {
    setSearch('');
    setCategoryId(0);
  };

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
      </div>

      {/* Multi-parameter search: question text + category, combined */}
      <Form className="mb-3" onSubmit={e => e.preventDefault()}>
        <Row className="g-2 align-items-end">
          <Col md={6}>
            <Form.Label htmlFor="questionSearch">Search question text</Form.Label>
            <Form.Control
              id="questionSearch"
              type="search"
              placeholder="For example: React"
              value={search}
              onChange={e => setSearch(e.target.value)}
            />
          </Col>
          <Col md={4}>
            <Form.Label htmlFor="categoryFilter">Category</Form.Label>
            <Form.Select
              id="categoryFilter"
              value={categoryId}
              onChange={e => setCategoryId(Number(e.target.value))}
            >
              <option value={0}>All categories</option>
              {categories.map(category => (
                <option key={category.categoryId} value={category.categoryId}>
                  {category.name}
                </option>
              ))}
            </Form.Select>
          </Col>
          <Col md={2}>
            <Button variant="outline-secondary" className="w-100" onClick={resetFilters} disabled={!isFiltering}>
              Reset
            </Button>
          </Col>
        </Row>
      </Form>

      {/* Conditional rendering (&&): only shown when an error exists */}
      {error && <Alert variant="danger">{error.message}</Alert>}
      {deleteMutation.error && <Alert variant="danger">{deleteMutation.error.message}</Alert>}

      {/* Conditional rendering (ternary): message when the list is empty, otherwise the table */}
      {!isLoading && !error && questions.length === 0 ? (
        // No-results feedback: different message when a filter is active
        isFiltering ? (
          <Alert variant="info">
            No questions match your search.{' '}
            <Alert.Link as="button" onClick={resetFilters}>Reset filters</Alert.Link>
          </Alert>
        ) : (
          <p>No questions found.</p>
        )
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