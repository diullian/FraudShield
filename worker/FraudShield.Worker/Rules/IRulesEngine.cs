using FraudShield.Worker.Contracts;

namespace FraudShield.Worker.Rules;

public interface IRulesEngine
{
    Task<FraudEvaluationResult> ValidateTransaction(TransactionCreatedEvent transaction);
}
