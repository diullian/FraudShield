using FraudShield.Worker;
using FraudShield.Worker.Contracts;
using FraudShield.Worker.Validation;
using MassTransit;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddScoped<IEventValidator, EventValidator>();

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
            //e.UseRawJsonDeserializer();
            e.ConfigureConsumer<FraudWorker>(context);
        });

        //cfg.ConfigureEndpoints(context);
    });
});


var host = builder.Build();
host.Run();
