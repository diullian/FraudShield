using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraudShield.Worker.Context;

public interface ICorrelationContext
{
    string CorrelationId { get; }
}
