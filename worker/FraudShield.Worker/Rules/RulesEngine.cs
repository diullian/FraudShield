using FraudShield.Worker.Contracts;
using FraudShield.Worker.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraudShield.Worker.Rules;

public class RulesEngine : IRulesEngine
{
    private readonly IEnumerable<IFraudRule> _rules;
    public RulesEngine(IEnumerable<IFraudRule> rules)
    {
        _rules = rules;
    }

    public async Task<FraudEvaluationResult> ValidateTransaction(TransactionCreatedEvent transaction)
    {
        var evaluationResult = new FraudEvaluationResult
        {
            TransactionId = transaction.TransactionId,
            CorrelationId = transaction.CorrelationId,
            EvaluatedAt = DateTime.UtcNow,
            TriggeredRules = new List<RuleResult>()
        };

        if (transaction != null)
        {
            _rules.ToList().ForEach(rule =>
            {
                if (rule.Evaluate(transaction) is RuleResult result && result.Triggered)
                {
                    evaluationResult.TriggeredRules.Add(result);
                }
            });
        }

        if(evaluationResult.TriggeredRules.Any())
        {
            evaluationResult.Decision = CalculateDecision(evaluationResult.TriggeredRules);
            evaluationResult.RiskLevel = CalculateRiskLevel(evaluationResult.TriggeredRules);
        }
        else
        {
            evaluationResult.Decision = FraudDecision.Approved;
            evaluationResult.RiskLevel = RiskLevel.Low;
        }

        return await Task.FromResult(evaluationResult);
    }
    private FraudDecision CalculateDecision(List<RuleResult> results)
    {
        if(results.Any(r => r.Decision == FraudDecision.Rejected))
        {
            return FraudDecision.Rejected;
        }
        else if(results.Any(r => r.Decision == FraudDecision.Review))
        {
            return FraudDecision.Review;
        }
        else
        {
            return FraudDecision.Approved;
        }
    }

    private RiskLevel CalculateRiskLevel(List<RuleResult> results)
    {
        if (results.Any(r => r.RiskLevel == RiskLevel.High))
        {
            return RiskLevel.High;
        }
        else if (results.Any(r => r.RiskLevel == RiskLevel.Medium))
        {
            return RiskLevel.Medium;
        }
        else
        {
            return RiskLevel.Low;
        }
    }
}
