using FraudShield.Worker.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraudShield.Worker.Rules;

public class FraudEvaluationResult
{
    public Guid TransactionId { get; set; }
    public FraudDecision Decision { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public List<RuleResult> TriggeredRules { get; set; } = [];
    public DateTime EvaluatedAt { get; set; }
}

