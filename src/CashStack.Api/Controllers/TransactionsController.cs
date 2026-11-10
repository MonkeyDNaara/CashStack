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
    public async Task<ActionResult<List<TransactionResponse>>> GetAll(
        [FromQuery] DateRangeQuery range, [FromQuery] TransactionType? type, CancellationToken ct)
    {
        IReadOnlyList<Transaction> transactions = await _service.GetTransactionsAsync(range.Start, range.End, type, ct);
        return Ok(transactions.Select(TransactionResponse.From).ToList());
    }

    [HttpGet("summary")]
    [ValidateDateRange]
    public async Task<ActionResult<SummaryResponse>> GetSummary([FromQuery] DateRangeQuery range, CancellationToken ct)
    {
        TransactionSummary summary = await _service.GetSummaryAsync(range.Start, range.End, ct);
        return Ok(SummaryResponse.Create(range.Start, range.End, summary));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TransactionResponse>> GetById(Guid id, CancellationToken ct)
    {
        Transaction? transaction = await _service.GetByIdAsync(id, ct);
        if (transaction is null)
        {
            return NotFound();
        }
        return Ok(TransactionResponse.From(transaction));
    }

    [HttpPost]
    public async Task<ActionResult<TransactionResponse>> Create(CreateTransactionRequest request, CancellationToken ct)
    {
        Transaction transaction = await _service.AddAsync(request.Type!.Value, request.Description, request.Amount, ct: ct);
        var response = TransactionResponse.From(transaction);
        return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        return await _service.RemoveAsync(id, ct) ? NoContent() : NotFound();
    }
}
