import { Link } from 'react-router-dom';

export default function AdminDashboardPage() {
  return (
    <div>
      <h1>Admin Dashboard</h1>
      <p>Manage the categories, questions, and quizzes used in Monster Munch.</p>

      <div className="row g-3">
        <div className="col-md-4">
          <div className="card h-100">
            <div className="card-body">
              <h2 className="h5">Questions</h2>
              <p>View and manage quiz questions.</p>
              <Link to="/admin/questions" className="btn btn-primary">
                Manage Questions
              </Link>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}