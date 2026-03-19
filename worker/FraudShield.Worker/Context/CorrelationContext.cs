using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraudShield.Worker.Context;

public class CorrelationContext : ICorrelationContext
{
    public string CorrelationId { get; private set; } = Guid.NewGuid().ToString();

    public void Initialize(string? correlationId)
    {
        CorrelationId = string.IsNullOrEmpty(correlationId) ? Guid.NewGuid().ToString() : correlationId;
    }
}
