using FraudShield.Worker.Contracts;
using FraudShield.Worker.Rules;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraudShield.Worker.Audit;

public class FraudAuditDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string _id { get; set; }

    public Guid TransactionId { get; set; }
    public Guid CorrelationId { get; set; }
    public string IdempotencyKey { get; set; }

    //snapshot do evento recebido
    public TransactionSnapshot Transaction{ get; set; }
    
    //resultado avaliação
    public string Decision { get; set; }
    public string RiskLevel { get; set; }

    //regras que foram aplicadas
    public List<RuleResult> AppliedRules { get; set; }
    

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime EvaluatedAt { get; set; }

    public static FraudAuditDocument From(
        TransactionCreatedEvent transaction,
        FraudEvaluatedResultEvent result,
        List<RuleResult> TriggeredRules)
    {
        return new FraudAuditDocument
        {
            TransactionId = transaction.TransactionId,
            CorrelationId = transaction.CorrelationId,
            IdempotencyKey = transaction.IdempotencyKey,
            Transaction = TransactionSnapshot.From(transaction),
            Decision = result.Status,
            RiskLevel = result.RiskLevel.ToString(),
            EvaluatedAt = DateTime.UtcNow,
            AppliedRules = TriggeredRules
        };
    }
}

