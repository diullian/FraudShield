using FraudShield.Application.Messaging;
using FraudShield.Contracts.Events;
using FraudShield.Domain.Repositories.Transactions;
using FraudShield.Infrastructure.DataAccess;
using FraudShield.Infrastructure.DataAccess.Repositories;
using FraudShield.Infrastructure.Messaging;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FraudShield.Infrastructure;

public static class DependencyInjectionExtension
{

    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddRepositories(services);
        AddDbContext(services, configuration);
        AddMessaging(services, configuration);
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<ITransactionsWriteOnlyRepository, TransactionsRepository>();
        services.AddScoped<IMessageBus, RabbitMqMessageBus>();
    }

    private static void AddDbContext(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<FraudShieldDbContext>(options => 
            options.UseSqlServer(configuration.GetConnectionString("Default")));
    }
    
    public static void AddMessaging(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMq:Host"], "/", h =>
                {
                    h.Username(configuration["RabbitMq:Username"]);
                    h.Password(configuration["RabbitMq:Password"]);
                });

                cfg.Message<TransactionCreatedEvent>(x =>
                {
                    x.SetEntityName("antifraude-validator");
                });
            });

        });
    }
}
