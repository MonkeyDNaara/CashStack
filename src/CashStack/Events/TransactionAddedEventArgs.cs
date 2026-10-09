using CashStack.Models;

namespace CashStack.Events;

public class TransactionAddedEventArgs: EventArgs
{
    public Transaction Transaction {get;}

    public TransactionAddedEventArgs(Transaction transaction)
    {
        Transaction = transaction;
    }
}