using FraudShield.Application.UseCases.Transaction;
using FraudShield.Communication.Requests;
using FraudShield.Domain.Repositories.Transactions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FraudShield.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TransactionsController : ControllerBase
{
    [HttpPost]
    public IActionResult Post(EvaluateTransactionUseCase useCase, RequestEvaluateTransactionJson request)
    {

        var response = useCase.ExecuteAsync(request);

        return Ok();
    }
}
