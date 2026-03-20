namespace FraudShield.Worker.Contracts;

public class FraudEvaluatedEvent
{
    public Guid TransactionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; }
    public string RiskLevel { get; set; }
    public DateTime ProcessedAt { get; set; }

}
