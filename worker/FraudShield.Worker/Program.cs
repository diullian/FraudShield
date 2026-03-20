using FraudShield.Worker;
using FraudShield.Worker.Audit.Repository;
using FraudShield.Worker.Context;
using FraudShield.Worker.Contracts;
using FraudShield.Worker.Rules;
using FraudShield.Worker.Validation;
using MassTransit;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddScoped<IEventValidator, EventValidator>();
builder.Services.AddScoped<IRulesEngine, RulesEngine>();

builder.Services.AddWorkerService(); //adicionando regras 
builder.Services.AddScoped<CorrelationContext>(); // contexto para armazenar o CorrelationId durante o processamento

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

//MongoDb configs
builder.Services.Configure<MongoSettings>(
    builder.Configuration.GetSection("MongoSettings"));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(settings.DatabaseName);
});

builder.Services.AddScoped<IAuditRepository, MongoAuditRepository>();


//masstransit configs
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
            e.UseMessageRetry(r => 
                r.Exponential(3, 
                    TimeSpan.FromSeconds(2), 
                    TimeSpan.FromSeconds(10), 
                    TimeSpan.FromSeconds(2))
            );

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
