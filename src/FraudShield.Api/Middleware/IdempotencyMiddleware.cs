using System.Text;
using System.Text.Json;
using FraudShield.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace FraudShield.Api.Middleware;

public class IdempotencyMiddleware
{
    private const string HeaderName = "Idempotency-Key";

    private readonly RequestDelegate _next;
    private readonly ILogger<IdempotencyMiddleware> _logger;

    public IdempotencyMiddleware(
        RequestDelegate next,
        ILogger<IdempotencyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IIdempotencyStore store)
    {
        if (!IsIdempotencyApplicable(context.Request.Method))
        {
            await _next(context);
            return;
        }

        var idempotencyKey = context.Request.Headers[HeaderName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            var errorJson = JsonSerializer.Serialize(new
            {
                message = $"{HeaderName} header is required."
            });

            await context.Response.WriteAsync(errorJson, Encoding.UTF8, context.RequestAborted);
            return;
        }

        IdempotencyEntry? cachedEntry = null;
        try
        {
            cachedEntry = await store.GetAsync(idempotencyKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read idempotency key from store: {IdempotencyKey}", idempotencyKey);
        }

        if (cachedEntry is not null)
        {
            _logger.LogInformation("Returning cached response for idempotency key {IdempotencyKey}", idempotencyKey);

            context.Response.StatusCode = cachedEntry.StatusCode;
            if (!string.IsNullOrWhiteSpace(cachedEntry.ContentType))
                context.Response.ContentType = cachedEntry.ContentType;

            await context.Response.WriteAsync(cachedEntry.Body ?? string.Empty, context.RequestAborted);
            return;
        }

        var originalBodyStream = context.Response.Body;
        await using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        await _next(context);

        if (context.Response.StatusCode is >= 200 and <= 299)
        {
            try
            {
                responseBodyStream.Position = 0;

                using var reader = new StreamReader(responseBodyStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
                var body = await reader.ReadToEndAsync();

                var entry = new IdempotencyEntry
                {
                    StatusCode = context.Response.StatusCode,
                    Body = body,
                    ContentType = context.Response.ContentType ?? string.Empty,
                    CreatedAt = DateTime.UtcNow
                };

                await store.SaveAsync(idempotencyKey, entry);
                _logger.LogInformation(
                    "Cached response for idempotency key {IdempotencyKey} with status {StatusCode}",
                    idempotencyKey,
                    context.Response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache idempotency response for key {IdempotencyKey}", idempotencyKey);
            }
        }

        context.Response.Body = originalBodyStream;
        responseBodyStream.Position = 0;
        await responseBodyStream.CopyToAsync(originalBodyStream, context.RequestAborted);
    }

    private static bool IsIdempotencyApplicable(string method)
    {
        return method.Equals(HttpMethods.Post, StringComparison.OrdinalIgnoreCase)
               || method.Equals("PATCH", StringComparison.OrdinalIgnoreCase)
               || method.Equals(HttpMethods.Put, StringComparison.OrdinalIgnoreCase);
    }
}

