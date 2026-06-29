using GreenhouseGuard.Application.Features.Anomalies.Queries.GetRecentAnomalies;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GreenhouseGuard.Api.Controllers;

[ApiController]
[Route("api/anomalies")]
public sealed class AnomaliesController : ControllerBase
{
    private readonly ISender _sender;

    public AnomaliesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> GetRecent(CancellationToken cancellationToken)
    {
        var anomalies = await _sender.Send(new GetRecentAnomaliesQuery(), cancellationToken);
        return Ok(anomalies);
    }
}