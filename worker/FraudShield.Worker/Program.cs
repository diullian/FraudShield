using FraudShield.Contracts.Events;
using FraudShield.Worker;
using FraudShield.Worker.Rules;
using FraudShield.Worker.Validation;
using MassTransit;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddScoped<IEventValidator, EventValidator>();
builder.Services.AddScoped<IRulesEngine, RulesEngine>();

builder.Services.AddWorkerService(); //adicionando regras 

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<FraudWorker>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMq:Username"]);
            h.Password(builder.Configuration["RabbitMq:Password"]);
        });

        cfg.ReceiveEndpoint("antifraude-validator", e =>
        {
            e.ConfigureConsumer<FraudWorker>(context);
        });


        cfg.Message<FraudEvaluatedResultEvent>(e =>
        {
            e.SetEntityName("fraud-evaluated-results");
        });

        //cfg.ConfigureEndpoints(context);
    });
});


var host = builder.Build();
host.Run();
