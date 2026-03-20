using FraudShield.Worker.Contracts;
using FraudShield.Worker.Enums;

namespace FraudShield.Worker.Rules.FraudRules.Amount;

public class MediumAmountRule : IFraudRule
{
    public string Name => "MediumAmount";

    private const decimal Threshold = 500m;
    private const decimal HighThreshold = 1000m;

    public RuleResult Evaluate(TransactionCreatedEvent transaction)
    {
        if (transaction.Amount > Threshold && transaction.Amount < HighThreshold)
        {
            return new RuleResult
            {
                Name = Name,
                Triggered = true,
                Decision = FraudDecision.Review,
                RiskLevel = RiskLevel.Low,
                Reason = $"Transaction amount {transaction.Amount} exceeds medium threshold of {Threshold}."
            };
        }

        return RuleResult.NotTriggered(Name);
    }
}
