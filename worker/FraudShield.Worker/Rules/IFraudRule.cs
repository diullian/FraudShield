using FraudShield.Contracts.Events;

namespace FraudShield.Worker.Rules;

public interface IFraudRule
{
    public string Name { get; }
    RuleResult Evaluate(TransactionCreatedEvent transaction);
}
