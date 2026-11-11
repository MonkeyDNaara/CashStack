import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';
import { renderRoute } from './test/renderRoute';

describe('app layout and routing', () => {
  it('shows the navigation with the dashboard as start page', () => {
    renderRoute('/');

    const nav = screen.getByRole('navigation', { name: 'Main' });
    expect(nav).toBeInTheDocument();
    expect(screen.getByRole('heading', { level: 1, name: 'Dashboard' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Dashboard' })).toHaveClass('menu-active');
  });

  it('switches pages through the navigation', async () => {
    const user = userEvent.setup();
    const { router } = renderRoute('/');

    await user.click(screen.getByRole('link', { name: 'Transactions' }));
    expect(screen.getByRole('heading', { level: 1, name: 'Transactions' })).toBeInTheDocument();
    expect(router.state.location.pathname).toBe('/transactions');

    await user.click(screen.getByRole('link', { name: 'Categories' }));
    expect(screen.getByRole('heading', { level: 1, name: 'Categories' })).toBeInTheDocument();
  });

  it('shows a not-found page for unknown routes', () => {
    renderRoute('/does-not-exist');

    expect(screen.getByRole('heading', { name: 'Page not found' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Back to the dashboard' })).toHaveAttribute(
      'href',
      '/',
    );
  });
});
