using FraudShield.Domain.Entities;
using FraudShield.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraudShield.Domain.Rules.FraudRules.Amount;

public class RuleHighAmount : IFraudRule
{
    public string Name => "HighAmount";

    private const decimal Threshold = 1000m;

    public RuleResult Evaluate(FinancialTransaction transaction)
    {
        if (transaction.Amount > Threshold)
        {
            return new RuleResult
            {
                Name = Name,
                Triggered = true,
                Decision = FraudDecision.Rejected,
                Reason = $"Transaction amount {transaction.Amount} exceeds the threshold."
            };
        }

        return RuleResult.NotTriggered(Name);
    }
}

