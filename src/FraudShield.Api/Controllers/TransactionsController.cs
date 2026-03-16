using FraudShield.Application.UseCases.Transaction;
using FraudShield.Communication.Requests;
using FraudShield.Communication.Responses;
using FraudShield.Domain.Repositories.Transactions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FraudShield.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TransactionsController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ResponseEvaluateTransactionJson), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post([FromServices] IEvaluateTransactionUseCase useCase, [FromBody] RequestEvaluateTransactionJson request, CancellationToken ct)
    {
        var response = useCase.ExecuteAsync(request, ct);

        return Ok(response.Result);
    }
}
