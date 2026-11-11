import { describe, expect, it } from 'vitest';
import { ApiError } from './ApiError';

describe('ApiError', () => {
  it('normalises ASP.NET field names to the camelCase names of the form', () => {
    const error = new ApiError(400, {
      title: 'One or more validation errors occurred.',
      errors: {
        Amount: ['Too many decimals.'],
        '$.type': ['The JSON value could not be converted.'],
        $: ['Invalid JSON.'],
      },
    });

    expect(error.fieldError('amount')).toBe('Too many decimals.');
    expect(error.fieldError('type')).toBe('The JSON value could not be converted.');
    expect(error.fieldError('request')).toBe('Invalid JSON.');
    expect(error.hasFieldErrors).toBe(true);
  });

  it('shows our detail text for 4xx errors', () => {
    const error = new ApiError(409, { title: 'Conflict', detail: "Category 'Rent' is in use." });

    expect(error.userMessage).toBe("Category 'Rent' is in use.");
    expect(error.isUnexpected).toBe(false);
  });

  it('asks to check the fields when a 400 has only field errors', () => {
    const error = new ApiError(400, { errors: { amount: ['Required.'] } });

    expect(error.userMessage).toBe('Please check the highlighted fields.');
  });

  it('never shows server details for 5xx errors', () => {
    const error = new ApiError(500, { title: 'Storage error', detail: 'Disk C:\\ full' });

    expect(error.isUnexpected).toBe(true);
    expect(error.userMessage).not.toContain('Disk');
  });

  it('marks network errors with status 0', () => {
    const error = ApiError.network(new TypeError('Failed to fetch'));

    expect(error.isNetworkError).toBe(true);
    expect(error.isUnexpected).toBe(true);
    expect(error.userMessage).toMatch(/not reachable/);
    expect(error.cause).toBeInstanceOf(TypeError);
  });
});
