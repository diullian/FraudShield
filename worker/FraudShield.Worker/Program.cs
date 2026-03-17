using FraudShield.Worker.Contracts;
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

        // serialização global pro Worker
        cfg.UseRawJsonSerializer(
            RawSerializerOptions.AnyMessageType,
            isDefault: true);

        cfg.ReceiveEndpoint("antifraude-validator", e =>
        {
            e.UseRawJsonDeserializer(RawSerializerOptions.AnyMessageType);
            e.ConfigureConsumer<FraudWorker>(context);
        });


        cfg.Message<FraudEvaluatedResultEvent>(e =>
        {
            e.SetEntityName("fraud-evaluated-results");
        });

        cfg.Publish<FraudEvaluatedResultEvent>(x =>
        {
            x.ExchangeType = "fanout";
        });
    });
});


var host = builder.Build();
host.Run();
