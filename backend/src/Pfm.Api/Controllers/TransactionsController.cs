using Microsoft.AspNetCore.Mvc;
using Pfm.Application.Common;
using Pfm.Application.Transactions;
using Pfm.Domain;

namespace Pfm.Api.Controllers;

/// <summary>Records and manages income and expenses.</summary>
[ApiController]
[Route("api/transactions")]
[Produces("application/json")]
public sealed class TransactionsController(TransactionService transactions) : ControllerBase
{
    /// <summary>Lists the transactions of one month, newest first.</summary>
    /// <param name="year">Calendar year, for example 2025.</param>
    /// <param name="month">Calendar month, 1 to 12.</param>
    /// <param name="type">Optional: return only income or only expenses.</param>
    /// <param name="categoryId">Optional: return only transactions of this category.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TransactionResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<TransactionResponse>>> GetAll(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] TransactionType? type,
        [FromQuery] string? categoryId,
        CancellationToken cancellationToken)
    {
        var filter = new TransactionFilter(MonthRange.Of(year, month), type, categoryId);
        return Ok(await transactions.GetAsync(filter, cancellationToken));
    }

    /// <summary>Returns a single transaction.</summary>
    /// <param name="id">Id of the transaction.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("{id}")]
    [ProducesResponseType<TransactionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionResponse>> GetById(string id, CancellationToken cancellationToken) =>
        Ok(await transactions.GetAsync(id, cancellationToken));

    /// <summary>Records a transaction.</summary>
    /// <response code="400">The category does not exist or does not match the given type.</response>
    [HttpPost]
    [ProducesResponseType<TransactionResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TransactionResponse>> Create(
        TransactionRequest request,
        CancellationToken cancellationToken)
    {
        var created = await transactions.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates a transaction.</summary>
    /// <response code="400">The category does not exist or does not match the given type.</response>
    [HttpPut("{id}")]
    [ProducesResponseType<TransactionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionResponse>> Update(
        string id,
        TransactionRequest request,
        CancellationToken cancellationToken) =>
        Ok(await transactions.UpdateAsync(id, request, cancellationToken));

    /// <summary>Deletes a transaction.</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await transactions.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
