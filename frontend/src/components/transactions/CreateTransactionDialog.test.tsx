import { screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { toIsoDate } from '../../lib/format';
import { ids, problems, transactions } from '../../test/fixtures';
import { renderRoute } from '../../test/renderRoute';
import { api, server } from '../../test/server';

/** Captures the body of the POST and answers 201 */
function recordCreate() {
  const bodies: unknown[] = [];
  server.use(
    http.post(api('/transactions'), async ({ request }) => {
      bodies.push(await request.json());
      return HttpResponse.json(transactions[0], { status: 201 });
    }),
  );
  return bodies;
}

async function openDialog(url = '/transactions?from=2026-11-01&to=2026-11-10') {
  const user = userEvent.setup();
  renderRoute(url);
  await user.click(await screen.findByRole('button', { name: /New transaction/ }));
  const dialog = await screen.findByRole('dialog', { name: 'New transaction' });
  return { user, dialog };
}

describe('CreateTransactionDialog', () => {
  it('creates an expense with amount, category, description and today as date', async () => {
    const bodies = recordCreate();
    const { user, dialog } = await openDialog();

    await user.type(within(dialog).getByLabelText('Amount in euro'), '42,50');
    await user.click(await within(dialog).findByRole('radio', { name: 'Groceries' }));
    await user.type(within(dialog).getByLabelText('Description'), 'Weekly shopping');
    await user.click(within(dialog).getByRole('button', { name: 'Save' }));

    expect(await screen.findByRole('status')).toHaveTextContent('Transaction added');
    expect(bodies).toEqual([
      {
        type: 'Expense',
        categoryId: ids.groceries,
        description: 'Weekly shopping',
        amount: 42.5,
        date: toIsoDate(new Date()),
      },
    ]);
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
  });

  it('validates in the browser before sending anything', async () => {
    const bodies = recordCreate();
    const { user, dialog } = await openDialog();

    await user.type(within(dialog).getByLabelText('Amount in euro'), '42,505');
    await user.click(within(dialog).getByRole('button', { name: 'Save' }));

    expect(within(dialog).getByText('At most 2 decimal places.')).toBeInTheDocument();
    expect(within(dialog).getByText('Choose a category.')).toBeInTheDocument();
    expect(within(dialog).getByText('Enter a description.')).toBeInTheDocument();
    expect(within(dialog).getByLabelText('Amount in euro')).toHaveAttribute('aria-invalid', 'true');
    // Not silently rounded to 42,51 on blur
    expect(within(dialog).getByLabelText('Amount in euro')).toHaveValue('42,505');
    expect(bodies).toHaveLength(0);
  });

  it('shows only the categories of the chosen type and clears the choice on switch', async () => {
    const { user, dialog } = await openDialog();

    await user.click(await within(dialog).findByRole('radio', { name: 'Groceries' }));
    await user.click(within(dialog).getByRole('button', { name: 'Income' }));

    expect(await within(dialog).findByRole('radio', { name: 'Salary' })).not.toBeChecked();
    expect(within(dialog).queryByRole('radio', { name: 'Groceries' })).not.toBeInTheDocument();
  });

  it('shows API field errors under the field', async () => {
    server.use(
      http.post(api('/transactions'), () =>
        HttpResponse.json(
          {
            ...problems.validation,
            errors: { date: ['The field Date must not be in the future.'] },
          },
          { status: 400 },
        ),
      ),
    );
    const { user, dialog } = await openDialog();

    await user.type(within(dialog).getByLabelText('Amount in euro'), '10');
    await user.click(await within(dialog).findByRole('radio', { name: 'Rent' }));
    await user.type(within(dialog).getByLabelText('Description'), 'Rent');
    await user.click(within(dialog).getByRole('button', { name: 'Save' }));

    expect(
      await within(dialog).findByText('The field Date must not be in the future.'),
    ).toBeInTheDocument();
    expect(within(dialog).getByLabelText('Date')).toHaveAttribute('aria-invalid', 'true');
    expect(within(dialog).queryByRole('alert')).not.toBeInTheDocument();
  });

  it('shows a business rule error as alert in the dialog', async () => {
    server.use(
      http.post(api('/transactions'), () =>
        HttpResponse.json(
          {
            title: 'Business rule violated',
            status: 400,
            detail: "Category 'Rent' does not exist.",
          },
          { status: 400 },
        ),
      ),
    );
    const { user, dialog } = await openDialog();

    await user.type(within(dialog).getByLabelText('Amount in euro'), '10');
    await user.click(await within(dialog).findByRole('radio', { name: 'Rent' }));
    await user.type(within(dialog).getByLabelText('Description'), 'Rent');
    await user.click(within(dialog).getByRole('button', { name: 'Save' }));

    expect(await within(dialog).findByRole('alert')).toHaveTextContent(
      "Category 'Rent' does not exist.",
    );
    expect(screen.getByRole('dialog')).toBeInTheDocument();
  });

  it('preselects the type filter of the list', async () => {
    const { dialog } = await openDialog('/transactions?type=Income');

    expect(within(dialog).getByRole('button', { name: 'Income' })).toHaveAttribute(
      'aria-pressed',
      'true',
    );
  });

  it('starts empty again after cancel', async () => {
    const { user, dialog } = await openDialog();
    await user.type(within(dialog).getByLabelText('Description'), 'Draft');
    await user.click(within(dialog).getByRole('button', { name: 'Cancel' }));

    await user.click(screen.getByRole('button', { name: /New transaction/ }));
    const reopened = await screen.findByRole('dialog', { name: 'New transaction' });
    await waitFor(() => expect(within(reopened).getByLabelText('Description')).toHaveValue(''));
  });

  it('can be opened from the dashboard', async () => {
    const { dialog } = await openDialog('/');

    expect(within(dialog).getByLabelText('Amount in euro')).toBeInTheDocument();
  });
});
