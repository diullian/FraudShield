using FraudShield.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<FraudWorker>();

var host = builder.Build();
host.Run();
