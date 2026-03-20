using FraudShield.Api.Middleware;
using FraudShield.Application;
using FraudShield.Application.Interfaces;
using FraudShield.Infrastructure;
using FraudShield.Infrastructure.DataAccess;
using FraudShield.Infrastructure.Http;
using FraudShield.Infrastructure.Idempotency;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });



// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


//Add infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

//Add application services
builder.Services.AddApplication();
builder.Services.AddScoped<ICorrelationContext, CorrelationContext>();

var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
});
builder.Services.AddScoped<IIdempotencyStore, RedisIdempotencyStore>();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<IdempotencyMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FraudShieldDbContext>();
    db.Database.Migrate(); // cria o banco e aplica migrations automaticamente
}

app.Run();

