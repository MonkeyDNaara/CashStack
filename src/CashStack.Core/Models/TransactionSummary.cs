namespace CashStack.Models;

public class TransactionSummary
{
    public decimal Income {get;}
    public decimal Expenses {get;}
    public decimal Balance => Income - Expenses;

    public TransactionSummary(decimal income, decimal expenses)
    {
        Income = income;
        Expenses = expenses;
    }
}