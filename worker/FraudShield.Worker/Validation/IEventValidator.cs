using FraudShield.Contracts.Events;

namespace FraudShield.Worker.Validation;

public interface IEventValidator
{
    EventValidationResult Validate(TransactionCreatedEvent transaction);
}
