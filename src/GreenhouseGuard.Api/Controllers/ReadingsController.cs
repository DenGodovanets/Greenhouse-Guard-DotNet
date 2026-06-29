using GreenhouseGuard.Api.Contracts;
using GreenhouseGuard.Application.Features.Readings.Commands.CreateReading;
using GreenhouseGuard.Application.Features.Readings.Queries.GetLatestReading;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GreenhouseGuard.Api.Controllers;

[ApiController]
[Route("api/readings")]
public sealed class ReadingsController : ControllerBase
{
    private readonly ISender _sender;

    public ReadingsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatest(CancellationToken cancellationToken)
    {
        var latest = await _sender.Send(new GetLatestReadingQuery(), cancellationToken);

        return latest is null ? NotFound() : Ok(latest);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateReadingRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreateReadingCommand(request.Temperature, request.Humidity, request.Co2Ppm),
            cancellationToken);

        return CreatedAtAction(nameof(GetLatest), null, result);
    }
}