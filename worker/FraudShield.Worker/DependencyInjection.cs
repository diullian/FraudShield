using FraudShield.Worker.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FraudShield.Worker;

public static class DependencyInjection
{
    public static IServiceCollection AddWorkerService(this IServiceCollection services)
    {

        AddRulesEngine(services);
        return services;
    }


    private static void AddRulesEngine(IServiceCollection services)
    {
        var fraudRules = typeof(IFraudRule).Assembly
            .GetTypes()
            .Where(t => t.IsClass &&
                        !t.IsAbstract &&
                        typeof(IFraudRule).IsAssignableFrom(t));

        foreach (var rule in fraudRules)
        {
            services.AddScoped(typeof(IFraudRule), rule);
        }

        services.AddScoped<IRulesEngine, RulesEngine>();
    }
}
