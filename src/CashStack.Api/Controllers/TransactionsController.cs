using CashStack.Api.Dtos;
using CashStack.Api.Filters;
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
    [ValidateDateRange]
    public ActionResult<List<TransactionResponse>> GetAll(
        [FromQuery] DateRangeQuery range, [FromQuery] TransactionType? type)
    {
        var transactions = _service.GetTransactions(range.Start, range.End, type)
            .Select(TransactionResponse.From)
            .ToList();
        return Ok(transactions);
    }

    [HttpGet("summary")]
    [ValidateDateRange]
    public ActionResult<SummaryResponse> GetSummary([FromQuery] DateRangeQuery range)
    {
        TransactionSummary summary = _service.GetSummary(range.Start, range.End);
        return Ok(SummaryResponse.Create(range.Start, range.End, summary));
    }

    [HttpGet("{id:guid}")]
    public ActionResult<TransactionResponse> GetById(Guid id)
    {
        Transaction? transaction = _service.GetById(id);
        if (transaction is null)
        {
            return NotFound();
        }
        return Ok(TransactionResponse.From(transaction));
    }

    [HttpPost]
    public ActionResult<TransactionResponse> Create(CreateTransactionRequest request)
    {
        Transaction transaction = _service.Add(request.Type!.Value, request.Description, request.Amount);
        var response = TransactionResponse.From(transaction);
        return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, response);
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        return _service.Remove(id) ? NoContent() : NotFound();
    }
}
