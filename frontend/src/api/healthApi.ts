import { HEALTH_URL } from '../config';
import { ApiError } from './ApiError';

/** GET /health → resolves when the API answers "Healthy", throws an ApiError otherwise */
export async function checkHealth(signal?: AbortSignal): Promise<void> {
  let response: Response;
  try {
    response = await fetch(HEALTH_URL, { signal });
  } catch (error) {
    if (error instanceof DOMException && error.name === 'AbortError') throw error;
    throw ApiError.network(error);
  }
  if (!response.ok) {
    throw await ApiError.fromResponse(response);
  }
}
