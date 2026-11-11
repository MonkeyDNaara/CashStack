import { screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import type { Category } from '../api/types';
import { categories, ids, problems } from '../test/fixtures';
import { renderRoute } from '../test/renderRoute';
import { api, server } from '../test/server';

const conflict = (detail: string) =>
  HttpResponse.json({ ...problems.conflict, detail }, { status: 409 });

async function column(name: 'Income' | 'Expenses') {
  return screen.findByRole('region', { name: new RegExp(`^${name}`) });
}

describe('CategoriesPage', () => {
  it('shows income and expense categories, "Other" last and locked', async () => {
    renderRoute('/categories');

    const expenses = await column('Expenses');
    const names = within(expenses)
      .getAllByRole('listitem')
      .map((li) => li.textContent);
    expect(names[0]).toContain('Groceries');
    expect(names.at(-1)).toContain('Protected');
    expect(
      within(expenses).queryByRole('button', { name: 'Rename Other' }),
    ).not.toBeInTheDocument();
    expect(within(await column('Income')).getByText('Salary')).toBeInTheDocument();
  });

  it('adds a category of the column type', async () => {
    const user = userEvent.setup();
    let body: unknown;
    let list: Category[] = categories;
    server.use(
      http.get(api('/categories'), () => HttpResponse.json(list)),
      http.post(api('/categories'), async ({ request }) => {
        body = await request.json();
        const created: Category = { id: 'new', name: 'Bonus', type: 'Income', isProtected: false };
        list = [...categories, created];
        return HttpResponse.json(created, { status: 201 });
      }),
    );
    renderRoute('/categories');

    const income = await column('Income');
    await user.type(within(income).getByLabelText('New income category'), '  Bonus {Enter}');

    expect(body).toEqual({ name: 'Bonus', type: 'Income' });
    expect(await within(income).findByText('Bonus')).toBeInTheDocument();
    expect(within(income).getByLabelText('New income category')).toHaveValue('');
  });

  it('shows the API message for a duplicate name at the field', async () => {
    const user = userEvent.setup();
    server.use(
      http.post(api('/categories'), () =>
        conflict("An Expense category named 'rent' already exists."),
      ),
    );
    renderRoute('/categories');

    const expenses = await column('Expenses');
    await user.type(within(expenses).getByLabelText('New expense category'), 'rent{Enter}');

    expect(await within(expenses).findByText(/named 'rent' already exists/)).toBeInTheDocument();
    expect(within(expenses).getByLabelText('New expense category')).toHaveAttribute(
      'aria-invalid',
      'true',
    );
  });

  it('validates an empty name without a request', async () => {
    const user = userEvent.setup();
    let calls = 0;
    server.use(
      http.post(api('/categories'), () => {
        calls++;
        return HttpResponse.json({}, { status: 201 });
      }),
    );
    renderRoute('/categories');

    const income = await column('Income');
    await user.click(within(income).getByRole('button', { name: 'Add' }));

    expect(within(income).getByText('Enter a name.')).toBeInTheDocument();
    expect(calls).toBe(0);
  });

  it('renames inline with Enter and cancels with Escape', async () => {
    const user = userEvent.setup();
    let body: unknown;
    server.use(
      http.put(api('/categories/:id'), async ({ request }) => {
        body = await request.json();
        return HttpResponse.json({ ...categories[3], name: 'Housing' });
      }),
    );
    renderRoute('/categories');
    const expenses = await column('Expenses');

    await user.click(within(expenses).getByRole('button', { name: 'Rename Groceries' }));
    await user.keyboard('{Escape}');
    expect(within(expenses).queryByLabelText('New name for Groceries')).not.toBeInTheDocument();

    await user.click(within(expenses).getByRole('button', { name: 'Rename Rent' }));
    const input = within(expenses).getByLabelText('New name for Rent');
    expect(input).toHaveFocus();
    await user.clear(input);
    await user.type(input, 'Housing{Enter}');

    expect(body).toEqual({ name: 'Housing' });
    expect(await screen.findByRole('status')).toHaveTextContent('Renamed to "Housing"');
  });

  it('keeps the row in edit mode with the API message on a rename conflict', async () => {
    const user = userEvent.setup();
    server.use(
      http.put(api('/categories/:id'), () =>
        conflict("An Expense category named 'Groceries' already exists."),
      ),
    );
    renderRoute('/categories');
    const expenses = await column('Expenses');

    await user.click(within(expenses).getByRole('button', { name: 'Rename Rent' }));
    const input = within(expenses).getByLabelText('New name for Rent');
    await user.clear(input);
    await user.type(input, 'Groceries{Enter}');

    expect(
      await within(expenses).findByText(/named 'Groceries' already exists/),
    ).toBeInTheDocument();
    expect(input).toBeInTheDocument();
  });

  it('does not send a request when the name did not change', async () => {
    const user = userEvent.setup();
    let calls = 0;
    server.use(
      http.put(api('/categories/:id'), () => {
        calls++;
        return HttpResponse.json(categories[3]);
      }),
    );
    renderRoute('/categories');
    const expenses = await column('Expenses');

    await user.click(within(expenses).getByRole('button', { name: 'Rename Rent' }));
    await user.click(within(expenses).getByRole('button', { name: 'Save' }));

    expect(calls).toBe(0);
    expect(within(expenses).queryByLabelText('New name for Rent')).not.toBeInTheDocument();
  });

  it('deletes after confirmation', async () => {
    const user = userEvent.setup();
    let deleted: string | undefined;
    server.use(
      http.delete(api('/categories/:id'), ({ params }) => {
        deleted = String(params.id);
        return new HttpResponse(null, { status: 204 });
      }),
    );
    renderRoute('/categories');

    await user.click(within(await column('Expenses')).getByRole('button', { name: 'Delete Rent' }));
    const dialog = screen.getByRole('dialog', { name: 'Delete category?' });
    await user.click(within(dialog).getByRole('button', { name: 'Delete' }));

    expect(await screen.findByRole('status')).toHaveTextContent('Category "Rent" deleted');
    expect(deleted).toBe(ids.rent);
    await waitFor(() => expect(screen.queryByRole('dialog')).not.toBeInTheDocument());
  });

  it('explains in the dialog why a category in use cannot be deleted', async () => {
    const user = userEvent.setup();
    server.use(
      http.delete(api('/categories/:id'), () =>
        conflict("Category 'Groceries' is used by transactions and cannot be deleted."),
      ),
    );
    renderRoute('/categories');

    await user.click(
      within(await column('Expenses')).getByRole('button', { name: 'Delete Groceries' }),
    );
    const dialog = screen.getByRole('dialog');
    await user.click(within(dialog).getByRole('button', { name: 'Delete' }));

    expect(await within(dialog).findByRole('alert')).toHaveTextContent('is used by transactions');
    expect(within(dialog).getByText(/Delete its transactions first/)).toBeInTheDocument();
    expect(within(dialog).queryByRole('button', { name: 'Delete' })).not.toBeInTheDocument();
    expect(within(dialog).getByRole('button', { name: 'Close' })).toBeInTheDocument();
  });

  it('shows an error alert with retry when categories cannot be loaded', async () => {
    const user = userEvent.setup();
    server.use(http.get(api('/categories'), () => HttpResponse.error()));
    renderRoute('/categories');

    const alert = await screen.findByRole('alert');
    expect(alert).toHaveTextContent('not reachable');

    server.resetHandlers();
    await user.click(within(alert).getByRole('button', { name: 'Try again' }));
    expect(await column('Expenses')).toBeInTheDocument();
  });
});
