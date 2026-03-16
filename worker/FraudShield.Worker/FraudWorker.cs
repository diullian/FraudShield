using FraudShield.Contracts.Events;
using FraudShield.Worker.Validation;
using MassTransit;
using System.Text.Json;

namespace FraudShield.Worker;

public class FraudWorker : IConsumer<TransactionCreatedEvent>
{
    private readonly ILogger<FraudWorker> _logger;
    private readonly IEventValidator _eventValidator;

    public FraudWorker(ILogger<FraudWorker> logger, IEventValidator validator)
    {
        _logger = logger;
        _eventValidator = validator;

    }

    public async Task Consume(ConsumeContext<TransactionCreatedEvent> context)
    {
        var transaction = context.Message;

        var validation = _eventValidator.Validate(transaction);


        if (!validation.IsValid)
        {

            _logger.LogWarning(
                "Invalid contract: {Errors}",
                string.Join(", ", validation.ErrorMessage));
            return;
        }

        _logger.LogInformation("Received transaction: {TransactionId}, Amount: {Amount} {Currency}, Customer: {CustomerEmail}",
                transaction.TransactionId, transaction.Amount, transaction.Currency, transaction.CustomerEmail);

        await Task.CompletedTask;
    }

}
