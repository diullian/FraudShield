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
    public string Message { get; set; }
    public TransactionStatus Status { get; set; }

}
