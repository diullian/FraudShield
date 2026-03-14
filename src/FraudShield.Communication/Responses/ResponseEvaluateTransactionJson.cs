using FraudShield.Communication.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraudShield.Communication.Responses;

public class ResponseEvaluateTransactionJson
{
    public Guid TransactionId { get; set; }
    public FraudDecision FraudDecision { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public List<string> TriggeredRules { get; set; } = []; 
    public DateTime EvaluatedAt { get; set; }

}
