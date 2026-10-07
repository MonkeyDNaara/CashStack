using CashStack.Services;

var storage = new StorageService("data");
var transactions = new TransactionService(storage);