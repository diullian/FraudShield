using FraudShield.Application.Messaging;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraudShield.Infrastructure.Messaging;

public class RabbitMqMessageBus : IMessageBus
{

    private readonly IPublishEndpoint _publishEndpoint;
    public RabbitMqMessageBus(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }


    public async Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class
    {
        await _publishEndpoint.Publish(message, ct);
    }
}
