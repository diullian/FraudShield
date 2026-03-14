using FraudShield.Worker.Contracts;
using FraudShield.Worker.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraudShield.Worker.Rules.FraudRules.Amount;

public class NightHighAmountRule : IFraudRule
{
    public string Name => "NightHighAmount";
    public const decimal NightThreshold = 500m;
    public const int NightStartHour = 0; // 00:00
    public const int NightEndHour = 6;   // 06:00

    public RuleResult Evaluate(TransactionCreatedEvent transaction)
    {
        var hour = transaction.CreatedAt.Hour;

        if (hour >= NightStartHour && hour < NightEndHour && transaction.Amount > NightThreshold)
        {
            return new RuleResult
            {
                Name = Name,
                Triggered = true,
                Decision = FraudDecision.Rejected,
                Reason = "Transaction amount is high during night hours."
            };
        }

        return RuleResult.NotTriggered(Name);
    }
}
