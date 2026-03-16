using FraudShield.Contracts.Events;

namespace FraudShield.Worker.Validation;

public class EventValidator : IEventValidator
{
    public EventValidationResult Validate(TransactionCreatedEvent transaction)
    {
        if(transaction is null)
        {
            return new EventValidationResult
            {
                TransactionId = Guid.Empty,
                IsValid = false,
                ErrorMessage = new List<string> { "Transaction cannot be null." }
            };
        }

        var ErrorMessages = new List<string>();

        if (transaction.TransactionId == Guid.Empty)
        {
            ErrorMessages.Add("TransactionId is required.");
        }

        if (transaction.Amount <= 0)
        {
            ErrorMessages.Add("Amount must be greater than zero.");
        }

        if(string.IsNullOrEmpty(transaction.PaymentType))
        {
            ErrorMessages.Add("PaymentType is required.");
        }

        if(string.IsNullOrEmpty(transaction.CustomerDocument))
        {
            ErrorMessages.Add("CustomerDocument is required.");
        }

        if(transaction.CreatedAt == default) 
        {
            ErrorMessages.Add("CreatedAt is required.");
        }

        return new EventValidationResult
        {
            TransactionId = transaction.TransactionId,
            IsValid = ErrorMessages.Count == 0,
            ErrorMessage  = ErrorMessages
        };
    }
}
