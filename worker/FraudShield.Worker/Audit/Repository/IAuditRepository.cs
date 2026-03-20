namespace FraudShield.Worker.Audit.Repository;

public interface IAuditRepository
{
    Task SaveAsync(FraudAuditDocument document, CancellationToken ct = default);
}
