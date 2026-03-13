using FraudShield.Communication.Requests;
using FraudShield.Communication.Responses;

namespace FraudShield.Application.UseCases.Transaction;

public interface IEvaluateTransactionUseCase
{
    Task<ResponseEvaluateTransactionJson> ExecuteAsync(RequestEvaluateTransactionJson request);
}
