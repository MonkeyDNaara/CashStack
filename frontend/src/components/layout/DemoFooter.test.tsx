import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { REPO_URL } from '../../config';
import { DemoFooter } from './DemoFooter';

describe('DemoFooter', () => {
  it('says that the data is shared and links to the code', () => {
    render(<DemoFooter />);

    expect(screen.getByRole('contentinfo')).toHaveTextContent(
      /data is shared with other visitors/i,
    );
    expect(screen.getByRole('link', { name: /code on github/i })).toHaveAttribute('href', REPO_URL);
  });
});
