using Application.Features.Discovery.Queries.GetSemanticSearch;
using MediatR;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class DiscoveryController : ControllerBase
{
    private readonly IMediator _mediator;

    public DiscoveryController(IMediator mediator) => _mediator = mediator;

    [HttpPost("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExecuteSemanticSearch(
        [FromBody] GetSemanticSearchQuery query,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(query, cancellationToken);
        return Ok(response);
    }
}
