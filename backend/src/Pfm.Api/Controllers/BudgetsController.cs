using Microsoft.AspNetCore.Mvc;
using Pfm.Application.Budgets;
using Pfm.Application.Common;

namespace Pfm.Api.Controllers;

/// <summary>Manages the monthly spending limit of an expense category.</summary>
[ApiController]
[Route("api/budgets")]
[Produces("application/json")]
public sealed class BudgetsController(BudgetService budgets) : ControllerBase
{
    /// <summary>Lists the budgets of one month.</summary>
    /// <param name="year">Calendar year, for example 2025.</param>
    /// <param name="month">Calendar month, 1 to 12.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<BudgetResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<BudgetResponse>>> GetForMonth(
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken cancellationToken) =>
        Ok(await budgets.GetForMonthAsync(MonthRange.Of(year, month), cancellationToken));

    /// <summary>
    /// Sets the limit for a category and month, creating the budget if it does not exist yet.
    /// </summary>
    /// <response code="400">The category does not exist or is not an expense category.</response>
    [HttpPut]
    [ProducesResponseType<BudgetResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BudgetResponse>> Upsert(
        UpsertBudgetRequest request,
        CancellationToken cancellationToken) =>
        Ok(await budgets.UpsertAsync(request, cancellationToken));

    /// <summary>Removes the limit of a category for one month.</summary>
    /// <param name="id">Id of the budget.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await budgets.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
