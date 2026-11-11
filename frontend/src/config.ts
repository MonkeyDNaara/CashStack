const DEFAULT_API_URL = 'http://localhost:5198';

// Trailing slashes are removed so paths can always start with "/"
export const API_URL = (import.meta.env.VITE_API_URL ?? DEFAULT_API_URL).replace(/\/+$/, '');
export const API_BASE = `${API_URL}/api/v1`;
