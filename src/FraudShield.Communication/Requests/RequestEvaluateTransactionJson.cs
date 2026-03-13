using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraudShield.Communication.Requests;

public class RequestEvaluateTransactionJson
{
    public string IdempotencyKey { get; set; }

    public decimal Amount { get; set; }

    public DateTime CreateAt { get; set; }

    public CustomerRequest Customer { get; set; }

    public MerchantRequest Merchant { get; set; }
}
