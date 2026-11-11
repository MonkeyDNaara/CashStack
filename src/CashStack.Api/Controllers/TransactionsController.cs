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
    private readonly CategoryService _categories;

    public TransactionsController(TransactionService service, CategoryService categories)
    {
        _service = service;
        _categories = categories;
    }

    [HttpGet]
    [ValidateDateRange]
    public async Task<ActionResult<List<TransactionResponse>>> GetAll(
        [FromQuery] DateRangeQuery range, [FromQuery] TransactionType? type, [FromQuery] Guid? categoryId,
        CancellationToken ct)
    {
        IReadOnlyList<Transaction> transactions =
            await _service.GetTransactionsAsync(range.Start, range.End, type, categoryId, ct);
        Dictionary<Guid, string> names = await GetCategoryNamesAsync(ct);
        return Ok(transactions.Select(t => ToResponse(t, names)).ToList());
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
        return Ok(ToResponse(transaction, await GetCategoryNamesAsync(ct)));
    }

    [HttpPost]
    public async Task<ActionResult<TransactionResponse>> Create(CreateTransactionRequest request, CancellationToken ct)
    {
        Transaction transaction = await _service.AddAsync(
            request.Type!.Value, request.Description, request.Amount, request.CategoryId!.Value, request.Date, ct);
        var response = ToResponse(transaction, await GetCategoryNamesAsync(ct));
        return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        return await _service.RemoveAsync(id, ct) ? NoContent() : NotFound();
    }

    // One query for all category names instead of one per transaction
    private async Task<Dictionary<Guid, string>> GetCategoryNamesAsync(CancellationToken ct)
    {
        IReadOnlyList<Category> categories = await _categories.GetAllAsync(ct: ct);
        return categories.ToDictionary(c => c.Id, c => c.Name);
    }

    private static TransactionResponse ToResponse(Transaction transaction, Dictionary<Guid, string> names) =>
        TransactionResponse.From(transaction, names.GetValueOrDefault(transaction.CategoryId, "Unknown"));
}
