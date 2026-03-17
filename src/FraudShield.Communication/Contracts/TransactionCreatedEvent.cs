namespace FraudShield.Communication.Contracts;

public  class TransactionCreatedEvent
{

    public Guid TransactionId { get; set; }
    public string IdempotencyKey { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Currency { get; set; }
    public string PaymentType { get; set; }

    // Customer flat
    public string CustomerDocument { get; set; }
    public string CustomerEmail { get; set; }
    public string CustomerIpAddress { get; set; }
    public string CustomerDevice { get; set; }
    public string CustomerCountry { get; set; }

    // Merchant flat
    public string MerchantName { get; set; }
    public string MerchantCategory { get; set; }
    public string MerchantCountry { get; set; }
    public string MerchantState { get; set; }
    public string MerchantCity { get; set; }
}
