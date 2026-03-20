using MongoDB.Driver;

namespace FraudShield.Worker.Audit.Repository;

public class MongoAuditRepository : IAuditRepository
{
    private readonly IMongoCollection<FraudAuditDocument> _collection;

    public MongoAuditRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<FraudAuditDocument>("fraud_audits");
    }

    public async Task SaveAsync(FraudAuditDocument document, CancellationToken ct = default)
    {
        await _collection.InsertOneAsync(document, cancellationToken: ct);
    }
}
