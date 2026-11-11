import type { ProblemDetails } from './types';

export type FieldErrors = Record<string, string[]>;

const NETWORK_MESSAGE = 'The CashStack API is not reachable. Is it running on the configured URL?';

/**
 * One error type for every failed API call, so the UI never has to parse responses itself.
 * - status 0      → network error (API down, CORS blocked, offline)
 * - status 400    → validation / business rule (field errors and/or detail)
 * - status 404/409→ detail written by the API
 * - status >= 500 → generic message, details stay in the server log
 */
export class ApiError extends Error {
  readonly status: number;
  readonly title: string;
  readonly detail?: string;
  readonly fieldErrors: FieldErrors;
  readonly traceId?: string;

  constructor(status: number, problem: ProblemDetails = {}, options?: { cause?: unknown }) {
    const title = problem.title ?? (status === 0 ? 'Network error' : `Request failed (${status})`);
    super(problem.detail ?? title, options);
    this.name = 'ApiError';
    this.status = status;
    this.title = title;
    this.detail = problem.detail;
    this.fieldErrors = normalizeFieldErrors(problem.errors);
    this.traceId = problem.traceId;
  }

  static network(cause: unknown): ApiError {
    return new ApiError(0, { title: 'Network error', detail: NETWORK_MESSAGE }, { cause });
  }

  /** Reads a failed response; falls back gracefully when the body is not ProblemDetails JSON */
  static async fromResponse(response: Response): Promise<ApiError> {
    let problem: ProblemDetails = {};
    try {
      const body: unknown = await response.json();
      if (body && typeof body === 'object') {
        problem = body as ProblemDetails;
      }
    } catch {
      // Empty or non-JSON body (e.g. a proxy error page): keep the status only
    }
    return new ApiError(response.status, { ...problem, status: response.status });
  }

  get isNetworkError(): boolean {
    return this.status === 0;
  }

  get isServerError(): boolean {
    return this.status >= 500;
  }

  /** Errors the user cannot fix by changing the input → shown as toast */
  get isUnexpected(): boolean {
    return this.isNetworkError || this.isServerError;
  }

  get hasFieldErrors(): boolean {
    return Object.keys(this.fieldErrors).length > 0;
  }

  /** First message for one form field, e.g. fieldError('amount') */
  fieldError(field: string): string | undefined {
    return this.fieldErrors[field]?.[0];
  }

  /** Text for an alert: our detail for 4xx, a safe generic text otherwise */
  get userMessage(): string {
    if (this.isNetworkError) return NETWORK_MESSAGE;
    if (this.isServerError)
      return 'The server could not process the request. Please try again later.';
    if (this.detail) return this.detail;
    if (this.hasFieldErrors) return 'Please check the highlighted fields.';
    return this.title;
  }
}

/**
 * ASP.NET Core names fields by their JSON name ("amount"), JSON parse errors by path ("$.amount").
 * We use the plain camelCase name, so the form can look errors up by its own field names.
 */
function normalizeFieldErrors(errors: ProblemDetails['errors']): FieldErrors {
  const result: FieldErrors = {};
  if (!errors) return result;

  for (const [key, messages] of Object.entries(errors)) {
    const name = key.replace(/^\$\.?/, '');
    const field = name ? name.charAt(0).toLowerCase() + name.slice(1) : 'request';
    result[field] = [...(result[field] ?? []), ...messages];
  }
  return result;
}

export function isApiError(error: unknown): error is ApiError {
  return error instanceof ApiError;
}
