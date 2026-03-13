using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraudShield.Domain.Enums;

public enum PaymentType
{
    CreditCard = 0,
    DebitCard = 1,
    Pix = 2,
    Boleto = 3,
}