using FraudShield.Worker.Contracts;

namespace FraudShield.Worker.Audit;

public class TransactionSnapshot
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PaymentType { get; set; } = string.Empty;
    public string CustomerDocument { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerIpAddress { get; set; } = string.Empty;
    public string CustomerCountry { get; set; } = string.Empty;
    public string MerchantName { get; set; } = string.Empty;
    public string MerchantCategory { get; set; } = string.Empty;
    public string MerchantCountry { get; set; } = string.Empty;

    public static TransactionSnapshot From(TransactionCreatedEvent transaction)
    {
        return new TransactionSnapshot
        {
            Amount = transaction.Amount,
            Currency = transaction.Currency.ToString(),
            PaymentType = transaction.PaymentType.ToString(),
            CustomerDocument = transaction.CustomerDocument,
            CustomerEmail = transaction.CustomerEmail,
            CustomerIpAddress = transaction.CustomerIpAddress,
            CustomerCountry = transaction.CustomerCountry,
            MerchantName = transaction.MerchantName,
            MerchantCategory = transaction.MerchantCategory,
            MerchantCountry = transaction.MerchantCountry
        };
    }
}
