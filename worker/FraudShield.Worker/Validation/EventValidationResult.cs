namespace FraudShield.Worker.Validation;

public class EventValidationResult
{
    public Guid TransactionId {  get; set; }

    public bool IsValid { get; set; }

    public List<string> ErrorMessage { get; set; } = [];
}
