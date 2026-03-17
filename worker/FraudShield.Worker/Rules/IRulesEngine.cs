using FraudShield.Contracts.Events;

namespace FraudShield.Worker.Rules;

public interface IRulesEngine
{
    Task<FraudEvaluationResult> ValidateTransaction(TransactionCreatedEvent transaction);
}
