using CashStack.Services;
using CashStack.Models;

var storage = new StorageService("data");
var transactions = new TransactionService(storage);
var logger = new LoggerService("logs");
transactions.TransactionAdded += logger.OnTransactionAdded;