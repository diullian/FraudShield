using FraudShield.Worker.Contracts;

namespace FraudShield.Worker.Validation;

public interface IEventValidator
{
    EventValidationResult Validate(TransactionCreatedEvent transaction);
}
