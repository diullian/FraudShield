using FraudShield.Worker.Contracts;
using FraudShield.Worker.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraudShield.Worker.Rules.FraudRules.Amount;

public class HighAmountRule : IFraudRule
{
    public string Name => "HighAmount";

    private const decimal Threshold = 1000m;

    public RuleResult Evaluate(TransactionCreatedEvent transaction)
    {
        if (transaction.Amount > Threshold)
        {
            return new RuleResult
            {
                Name = Name,
                Triggered = true,
                Decision = FraudDecision.Rejected,
                RiskLevel = RiskLevel.High,
                Reason = $"Transaction amount {transaction.Amount} exceeds the threshold."
            };
        }

        return RuleResult.NotTriggered(Name);
    }
}
