using CashStack.Services;

var storage = new StorageService();
var transactions = new TransactionService(storage);