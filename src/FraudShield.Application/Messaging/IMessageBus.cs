namespace FraudShield.Application.Messaging;

public interface IMessageBus
{
    Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class;
}
