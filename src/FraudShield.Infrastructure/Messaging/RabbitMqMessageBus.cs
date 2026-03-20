using FraudShield.Application.Interfaces;
using FraudShield.Application.Messaging;
using MassTransit;

namespace FraudShield.Infrastructure.Messaging;

public class RabbitMqMessageBus : IMessageBus
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ICorrelationContext _correlationContext;
    public RabbitMqMessageBus(IPublishEndpoint publishEndpoint, ICorrelationContext correlationContext)
    {
        _publishEndpoint = publishEndpoint;
        _correlationContext = correlationContext;
    }

    public async Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class
    {
        await _publishEndpoint.Publish(message, ctx =>
        {
            if (Guid.TryParse(_correlationContext.CorrelationId, out var guid))
                ctx.CorrelationId = guid;
        }, ct);
    }
}
