using FraudShield.Domain.Repositories.Transactions;
using FraudShield.Infrastructure.DataAccess.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace FraudShield.Infrastructure;

public static class DependencyInjectionExtension
{

    public static void AddInfrastructure(this IServiceCollection services)
    {
        AddRepositories(services);
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<ITransactionsWriteOnlyRepository, TransactionsRepository>();
    }
}
