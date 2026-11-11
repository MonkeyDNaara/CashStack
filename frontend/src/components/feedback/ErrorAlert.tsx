import { isApiError } from '../../api/ApiError';

type ErrorAlertProps = {
  error: unknown;
  /** Shows a "Try again" button, e.g. refetch of a failed query */
  onRetry?: () => void;
  title?: string;
};

/** Alert for a failed page load or form submit, with the API's message for the user */
export function ErrorAlert({ error, onRetry, title = 'Something went wrong' }: ErrorAlertProps) {
  const message = isApiError(error) ? error.userMessage : 'An unexpected error occurred.';

  return (
    <div role="alert" className="alert alert-error alert-soft">
      <div>
        <h3 className="font-semibold">{title}</h3>
        <p className="text-sm">{message}</p>
      </div>
      {onRetry && (
        <button type="button" className="btn btn-sm" onClick={onRetry}>
          Try again
        </button>
      )}
    </div>
  );
}
