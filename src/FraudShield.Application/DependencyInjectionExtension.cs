using FraudShield.Application.Mapping;
using FraudShield.Application.UseCases.Transaction.GetAll;
using FraudShield.Application.UseCases.Transaction.GetById;
using FraudShield.Application.UseCases.Transaction.Register;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;

namespace FraudShield.Application;

public static class DependencyInjectionExtension
{

    public static void AddApplication(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;

        AddUseCases(services);

        FinancialTransactionMappingConfig.Register(config); //registra mapster DI

        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();
    }

    private static void AddUseCases(IServiceCollection services)
    {
        services.AddScoped<IEvaluateTransactionUseCase, EvaluateTransactionUseCase>();
        services.AddScoped<IGetAllTransactionsUseCase, GetAllTransactionsUseCase>();
        services.AddScoped<IGetTransactionsByIdUseCase, GetTransactionsByIdUseCase>();
    }


}
