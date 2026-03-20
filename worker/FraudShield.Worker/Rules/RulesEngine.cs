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


        // Valida todas as regras para a transação e coleta os resultados
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

        // Calcula a decisão final e o nível de risco com base nas regras acionadas
        if (evaluationResult.TriggeredRules.Any())
        {
            evaluationResult.Decision = CalculateDecision(evaluationResult.TriggeredRules);
            evaluationResult.RiskLevel = CalculateRiskLevel(evaluationResult.TriggeredRules);
        }
        else
        {
            evaluationResult.Decision = FraudDecision.Approved;
            evaluationResult.RiskLevel = RiskLevel.Low;
        }

        // Retorna o resultado da avaliação
        return await Task.FromResult(evaluationResult);
    }
    private FraudDecision CalculateDecision(List<RuleResult> results)
    {
        // Se alguma regra rejeitar a transação, a decisão final é rejeitada
        if (results.Any(r => r.Decision == FraudDecision.Rejected))
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
        // O nível de risco é determinado pelo nível mais alto entre as regras acionadas
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
