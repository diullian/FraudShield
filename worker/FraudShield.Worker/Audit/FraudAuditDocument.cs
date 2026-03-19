using FraudShield.Worker.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraudShield.Worker.Audit;

public class FraudAuditDocument
{
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }
    public string Decision { get; set; }
    public string RiskLevel { get; set; }

    public List<RuleResult> AppliedRules { get; set; }

    public DateTime EvaluatedAt { get; set; }

    public string? CorrelationId { get; set; }
}

