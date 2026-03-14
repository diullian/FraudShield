using FraudShield.Communication.Requests;
using FraudShield.Domain.Entities;
using Mapster;

namespace FraudShield.Application.Mapping;

public class TransactionMappingConfig
{
    public static void Register(TypeAdapterConfig config) 
    {
        // Example mapping configuration
        // config.NewConfig<RequestEvaluateTransactionJson, TransactionEntity>()
        //     .Map(dest => dest.Amount, src => src.Amount)
        //     .Map(dest => dest.CustomerDocument, src => src.Customer.Document)
        //     .Map(dest => dest.MerchantName, src => src.Merchant.Name);

        config.NewConfig<RequestEvaluateTransactionJson, FinancialTransaction>()
                .Map(dest => dest.Id, _ => Guid.NewGuid())
                .Map(dest => dest.CreatedAt, _ => DateTime.UtcNow)
                .Map(dest => dest.IdempotencyKey, src => src.IdempotencyKey)
                .Map(dest => dest.Amount, src => src.Amount)
                .Map(dest => dest.Currency, src => (Domain.Enums.Currency)src.Currency)
                .Map(dest => dest.PaymentType, src => (Domain.Enums.PaymentType)src.PaymentType)
                .Map(dest => dest.Customer, src => src.Customer)
                .Map(dest => dest.Merchant, src => src.Merchant);
    }
}
