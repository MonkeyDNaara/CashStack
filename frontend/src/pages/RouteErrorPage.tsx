import { isRouteErrorResponse, Link, useRouteError } from 'react-router';

// Last line of defence: shown when a page throws while rendering
export function RouteErrorPage() {
  const error = useRouteError();
  const message = isRouteErrorResponse(error)
    ? `${error.status} ${error.statusText}`
    : 'Something went wrong while showing this page.';

  return (
    <div className="hero min-h-screen bg-base-200">
      <div className="hero-content text-center">
        <div role="alert">
          <h1 className="text-3xl font-bold">Unexpected error</h1>
          <p className="py-4 text-base-content/70">{message}</p>
          <Link to="/" className="btn btn-primary">
            Back to the dashboard
          </Link>
        </div>
      </div>
    </div>
  );
}
