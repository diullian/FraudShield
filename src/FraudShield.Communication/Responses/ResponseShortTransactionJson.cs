using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraudShield.Communication.Responses;

public class ResponseShortTransactionJson
{
    public Guid TransactionId { get; set; }

    public decimal Amount { get; set; }

    public string Status { get; set; }

    public string RiskLevel { get; set; }

    public string PaymentType { get; set; }

    public string CustomerEmail { get; set; }

    public DateTime CreatedAt { get; set; }
}
