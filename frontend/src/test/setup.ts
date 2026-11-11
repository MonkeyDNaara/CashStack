import '@testing-library/jest-dom/vitest';
import { cleanup } from '@testing-library/react';
import { afterAll, afterEach, beforeAll } from 'vitest';
import { toast } from '../lib/toast';
import { server } from './server';

// Every request must be answered by a handler: a forgotten mock fails the test
beforeAll(() => server.listen({ onUnhandledFrame: 'error' }));

afterEach(() => {
  cleanup();
  server.resetHandlers();
  toast.clear();
});

afterAll(() => server.close());
