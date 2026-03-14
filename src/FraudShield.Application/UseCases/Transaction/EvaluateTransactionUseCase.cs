using FraudShield.Communication.Enums;
using FraudShield.Communication.Requests;
using FraudShield.Communication.Responses;
using FraudShield.Domain.Repositories.Transactions;

namespace FraudShield.Application.UseCases.Transaction;

public class EvaluateTransactionUseCase : IEvaluateTransactionUseCase
{

    private readonly ITransactionsWriteOnlyRepository _repository;
    public EvaluateTransactionUseCase(ITransactionsWriteOnlyRepository repository)
    {
        _repository = repository;
    }

    public Task<ResponseEvaluateTransactionJson> ExecuteAsync(RequestEvaluateTransactionJson request)
    {

        Validate(request);

        

        return Task.FromResult(new ResponseEvaluateTransactionJson
        {
            TransactionId = Guid.NewGuid(),
            FraudDecision = FraudDecision.Approved,
            RiskLevel = RiskLevel.Low,
            TriggeredRules = new List<string>(),
            EvaluatedAt = DateTime.UtcNow
        });

    }

    private void Validate(RequestEvaluateTransactionJson request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        if (request.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.", nameof(request.Amount));
        if (string.IsNullOrWhiteSpace(request.Customer.Document))
            throw new ArgumentException("Customer Document is required.", nameof(request.Customer.Document));
        if (string.IsNullOrWhiteSpace(request.Merchant.Name))
            throw new ArgumentException("Merchant Name is required.", nameof(request.Merchant.Name));
    }
}
