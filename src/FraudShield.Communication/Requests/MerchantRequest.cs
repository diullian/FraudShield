using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraudShield.Communication.Requests;

public class MerchantRequest
{
    public string Name { get; set; }
    public string Category { get; set; } // MCC code ex: "5411" = supermercado
    public string Country { get; init; }
    public string State { get; init; }
    public string City { get; init; }
}
