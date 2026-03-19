using System;
using System.Threading.Tasks;

namespace FraudShield.Application.Interfaces;

public class IdempotencyEntry
{
    public int StatusCode { get; set; }
    public string Body { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public interface IIdempotencyStore
{
    Task<IdempotencyEntry?> GetAsync(string key);
    Task SaveAsync(string key, IdempotencyEntry entry);
}

