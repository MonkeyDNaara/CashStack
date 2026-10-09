using CashStack.Services;
using CashStack.Models;

var storage = new StorageService("data");
var transactions = new TransactionService(storage);

