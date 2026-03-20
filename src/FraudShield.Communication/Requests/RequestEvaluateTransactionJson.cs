using FraudShield.Communication.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraudShield.Communication.Requests;

public class RequestEvaluateTransactionJson
{
    public decimal Amount { get; set; }

    public DateTime CreatedAt { get; set; }

    public Currency Currency { get; set; }

    public PaymentType PaymentType { get; set; }
    public RequestCustomerJson Customer { get; set; }
    public RequestMerchantJson Merchant { get; set; }
}
