using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraudShield.Communication.Enums;

public enum TransactionStatus
{
    Pending = 0,
    Approved = 1,
    Review = 2,
    Rejected = 3,
    Cancelled = 4
}