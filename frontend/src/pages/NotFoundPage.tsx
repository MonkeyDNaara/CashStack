import { Link } from 'react-router';

export function NotFoundPage() {
  return (
    <div className="hero py-16">
      <div className="hero-content text-center">
        <div>
          <h1 className="text-4xl font-bold">Page not found</h1>
          <p className="py-4 text-base-content/70">This page does not exist.</p>
          <Link to="/" className="btn btn-primary">
            Back to the dashboard
          </Link>
        </div>
      </div>
    </div>
  );
}
