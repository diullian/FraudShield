using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FraudShield.Application.Exceptions;

namespace FraudShield.Api.Exceptions;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        var (status, title, errors) = exception switch
        {
            ConflictException ex => (409, "Conflict", (IReadOnlyList<string>?)null),
            NotFoundException ex => (404, "Not Found", null),
            ValidationException ex => (422, "Validation Failed", ex.Errors),
            ArgumentException ex => (400, "Bad Request", null),
            _ => (500, "Internal Server Error", null)
        };

        httpContext.Response.StatusCode = status;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = exception.Message
        }, ct);

        return true;
    }
}
