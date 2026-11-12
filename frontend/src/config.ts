const DEFAULT_API_URL = 'http://localhost:5198';

// Trailing slashes are removed so paths can always start with "/"
export const API_URL = (import.meta.env.VITE_API_URL ?? DEFAULT_API_URL).replace(/\/+$/, '');
export const API_BASE = `${API_URL}/api/v1`;
// Liveness check of the API (outside /api/v1), used to detect a sleeping free server
export const HEALTH_URL = `${API_URL}/health`;

// Set only in the Render build: shows the live-demo footer and the "waking up" wording
export const DEMO_MODE = import.meta.env.VITE_DEMO_MODE === 'true';
export const REPO_URL = 'https://github.com/MonkeyDNaara/CashStack';
