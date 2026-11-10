using CashStack.Api.Dtos;
using CashStack.Models;
using CashStack.Services;
using Microsoft.AspNetCore.Mvc;

namespace CashStack.Api.Controllers;

// Business rules (unique names, protected "Other", in use) live in CategoryService.
// Their ConflictException becomes 409 in the GlobalExceptionHandler, so the actions stay thin.
[ApiController]
[Route("api/v1/categories")]
public class CategoriesController : ControllerBase
{
    private readonly CategoryService _service;

    public CategoriesController(CategoryService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<CategoryResponse>>> GetAll([FromQuery] TransactionType? type, CancellationToken ct)
    {
        IReadOnlyList<Category> categories = await _service.GetAllAsync(type, ct);
        return Ok(categories.Select(CategoryResponse.From).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoryResponse>> GetById(Guid id, CancellationToken ct)
    {
        Category? category = await _service.GetByIdAsync(id, ct);
        if (category is null)
        {
            return NotFound();
        }
        return Ok(CategoryResponse.From(category));
    }

    [HttpPost]
    public async Task<ActionResult<CategoryResponse>> Create(CreateCategoryRequest request, CancellationToken ct)
    {
        Category category = await _service.CreateAsync(request.Name, request.Type!.Value, ct);
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, CategoryResponse.From(category));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CategoryResponse>> Rename(Guid id, RenameCategoryRequest request, CancellationToken ct)
    {
        Category? category = await _service.RenameAsync(id, request.Name, ct);
        if (category is null)
        {
            return NotFound();
        }
        return Ok(CategoryResponse.From(category));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        return await _service.DeleteAsync(id, ct) ? NoContent() : NotFound();
    }
}
