using FraudShield.Application.UseCases.Transaction.GetAll;
using FraudShield.Application.UseCases.Transaction.GetById;
using FraudShield.Application.UseCases.Transaction.Register;
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
    public async Task<IActionResult> Post(
        [FromServices] IEvaluateTransactionUseCase useCase,
        [FromBody] RequestEvaluateTransactionJson request,
        [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
        CancellationToken ct)
    {
        var response = await useCase.ExecuteAsync(request, idempotencyKey, ct);
        return Ok(response);
    }


    [HttpGet]
    [Route("{id:guid}")]
    [ProducesResponseType(typeof(ResponseTransactionsJson), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status204NoContent)] 
    public async Task<IActionResult> GetById([FromServices] IGetTransactionsByIdUseCase useCase,[FromRoute] Guid id, CancellationToken ct)
    {
        var result = await useCase.Execute(id, ct);

        if(result != null)
        {
            return Ok(result);
        }

        return NoContent();
    }

    [HttpGet]
    [ProducesResponseType(typeof(ResponseTransactionsJson), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetAllTranscations(
        [FromServices] IGetAllTransactionsUseCase useCase,
        CancellationToken ct)
    {
        var transactions = await useCase.Execute(ct);

        if (transactions.Transactions.Count != 0)
        {
            return Ok(transactions);
        }

        return NoContent();
    }

}
