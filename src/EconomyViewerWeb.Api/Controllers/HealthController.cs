using Microsoft.AspNetCore.Mvc;

namespace EconomyViewerWeb.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "healthy",
            service = "EconomyViewerWeb.Api",
            timestamp = DateTimeOffset.UtcNow,
        });
    }
}
