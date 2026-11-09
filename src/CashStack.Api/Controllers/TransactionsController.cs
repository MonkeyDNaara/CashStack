using CashStack.Api.Dtos;
using CashStack.Models;
using CashStack.Services;
using Microsoft.AspNetCore.Mvc;

namespace CashStack.Api.Controllers;

[ApiController]
[Route("api/v1/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly TransactionService _service;

    public TransactionsController(TransactionService service)
    {
        _service = service;
    }

    [HttpGet]
    public ActionResult<List<TransactionResponse>> GetAll(
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] TransactionType? type)
    {
        var (start, end) = ResolveRange(from, to);
        var transactions = _service.GetTransactions(start, end, type)
            .Select(TransactionResponse.From)
            .ToList();
        return Ok(transactions);
    }

    [HttpGet("summary")]
    public ActionResult<SummaryResponse> GetSummary([FromQuery] DateOnly? from, [FromQuery] DateOnly? to)
    {
        var (start, end) = ResolveRange(from, to);
        TransactionSummary summary = _service.GetSummary(start, end);
        return Ok(SummaryResponse.Create(start, end, summary));
    }

    // Default range: first day of the month of 'to' (default today) until 'to'
    private static (DateOnly From, DateOnly To) ResolveRange(DateOnly? from, DateOnly? to)
    {
        DateOnly end = to ?? DateOnly.FromDateTime(DateTime.Now);
        DateOnly start = from ?? new DateOnly(end.Year, end.Month, 1);
        return (start, end);
    }
}
