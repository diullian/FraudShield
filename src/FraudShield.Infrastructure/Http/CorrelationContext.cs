using FraudShield.Application.Interfaces;

namespace FraudShield.Infrastructure.Http;

public class CorrelationContext : ICorrelationContext
{
    public string CorrelationId { get; set; } = default;
}
