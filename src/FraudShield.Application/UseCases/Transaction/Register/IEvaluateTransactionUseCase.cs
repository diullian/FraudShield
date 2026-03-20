using FraudShield.Communication.Requests;
using FraudShield.Communication.Responses;

namespace FraudShield.Application.UseCases.Transaction.Register;

public interface IEvaluateTransactionUseCase
{
    Task<ResponseEvaluateTransactionJson> ExecuteAsync(
        RequestEvaluateTransactionJson request,
        string idempotencyKey,
        CancellationToken ct = default);
}
