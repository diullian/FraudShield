using FraudShield.Application.Interfaces;
using FraudShield.Infrastructure.Http;

namespace FraudShield.Api.Middleware;

public class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ICorrelationContext correlation)
    {
        // ICorrelationContext é Scoped — instância única por request
        var id = context.Request.Headers[HeaderName].FirstOrDefault()
                 ?? Guid.NewGuid().ToString();

        ((CorrelationContext)correlation).CorrelationId = id;

        context.Response.Headers[HeaderName] = id;

        await _next(context);
    }
}