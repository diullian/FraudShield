using FraudShield.Communication.Requests;
using FraudShield.Contracts.Events;
using FraudShield.Domain.Entities;
using Mapster;

namespace FraudShield.Application.Mapping;

public class FinancialTransactionMappingConfig
{
    public static void Register(TypeAdapterConfig config) 
    {
        config.NewConfig<RequestEvaluateTransactionJson, FinancialTransaction>()
                .Map(dest => dest.Id, _ => Guid.NewGuid())
                .Map(dest => dest.CreatedAt, _ => DateTime.UtcNow)
                .Map(dest => dest.IdempotencyKey, src => src.IdempotencyKey)
                .Map(dest => dest.Amount, src => src.Amount)
                .Map(dest => dest.Currency, src => (Domain.Enums.Currency)src.Currency)
                .Map(dest => dest.PaymentType, src => (Domain.Enums.PaymentType)src.PaymentType)
                .Map(dest => dest.Customer, src => src.Customer)
                .Map(dest => dest.Merchant, src => src.Merchant);

        config.NewConfig<RequestCustomerJson, Customer>()
                .Map(dest => dest.Id, _ => Guid.NewGuid())
                .Map(dest => dest.DeviceType, src => (Domain.Enums.DeviceType)src.DeviceType);
       
        config.NewConfig<RequestMerchantJson, Merchant>()
                .Map(dest => dest.Id, _ => Guid.NewGuid());

        config.NewConfig<FinancialTransaction, TransactionCreatedEvent>()
                .Map(dest => dest.TransactionId, src => src.Id)
                .Map(dest => dest.IdempotencyKey, src => src.IdempotencyKey)
                .Map(dest => dest.Amount, src => src.Amount)
                .Map(dest => dest.CreatedAt, src => src.CreatedAt)
                .Map(dest => dest.Currency, src => src.Currency.ToString())
                .Map(dest => dest.PaymentType, src => src.PaymentType.ToString())
                .Map(dest => dest.CustomerDocument, src => src.Customer.Document)
                .Map(dest => dest.CustomerEmail, src => src.Customer.Email)
                .Map(dest => dest.CustomerIpAddress, src => src.Customer.IpAddress)
                .Map(dest => dest.CustomerDevice, src => src.Customer.DeviceType.ToString())
                .Map(dest => dest.CustomerCountry, src => src.Customer.Country)
                .Map(dest => dest.MerchantName, src => src.Merchant.Name)
                .Map(dest => dest.MerchantCategory, src => src.Merchant.Category)
                .Map(dest => dest.MerchantCountry, src => src.Merchant.Country)
                .Map(dest => dest.MerchantState, src => src.Merchant.State)
                .Map(dest => dest.MerchantCity, src => src.Merchant.City);

    }
}
