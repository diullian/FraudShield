using FraudShield.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraudShield.Domain.Rules;

public record RuleResult
{
    public string Name { get; set; }
    public bool Triggered { get; set; }
    public FraudDecision Decision { get; set; }

    public RiskLevel RiskLevel { get; set; }
    public string Reason { get; set; }

    public static RuleResult NotTriggered(string name)
    {
        return new RuleResult
        {
            Name = name,
            Triggered = false,
            Decision = FraudDecision.Approved,
            Reason = string.Empty
        };
    }
}
