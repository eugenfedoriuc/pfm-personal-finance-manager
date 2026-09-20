using Microsoft.AspNetCore.Mvc;
using Pfm.Application.Common;
using Pfm.Application.Summaries;

namespace Pfm.Api.Controllers;

/// <summary>Aggregated view of one month: totals, per-category breakdown and budget status.</summary>
[ApiController]
[Route("api/summaries")]
[Produces("application/json")]
public sealed class SummariesController(SummaryService summaries) : ControllerBase
{
    /// <summary>Returns the summary of one month.</summary>
    /// <param name="year">Calendar year, for example 2025.</param>
    /// <param name="month">Calendar month, 1 to 12.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("{year:int}/{month:int}")]
    [ProducesResponseType<MonthlySummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MonthlySummaryResponse>> Get(
        int year,
        int month,
        CancellationToken cancellationToken) =>
        Ok(await summaries.GetAsync(MonthRange.Of(year, month), cancellationToken));
}
