using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FraudShield.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TransactionsController : ControllerBase
{
    [HttpPost]
    public IActionResult Post()
    {
        return Ok();
    }
}
