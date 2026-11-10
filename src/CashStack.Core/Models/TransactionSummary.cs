namespace CashStack.Models;

public class TransactionSummary
{
    public decimal Income {get;}
    public decimal Expenses {get;}
    public decimal Balance => Income - Expenses;
    // Only categories with transactions in the period: Income first, then by total (largest first)
    public IReadOnlyList<CategoryTotal> Categories {get;}

    public TransactionSummary(decimal income, decimal expenses, IReadOnlyList<CategoryTotal> categories)
    {
        Income = income;
        Expenses = expenses;
        Categories = categories;
    }
}