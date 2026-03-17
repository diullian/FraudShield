//using FraudShield.Communication.Enums;
using FraudShield.Domain.Entities;
using FraudShield.Domain.Enums;
using FraudShield.Domain.Repositories.Transactions;
using Microsoft.EntityFrameworkCore;

namespace FraudShield.Infrastructure.DataAccess.Repositories;

internal class TransactionsRepository : ITransactionsWriteOnlyRepository, ITransactionsUpdateOnlyRepository
{
    private readonly FraudShieldDbContext _dbContext;

    public TransactionsRepository(FraudShieldDbContext dbContext)
    {
       _dbContext = dbContext;
    }

    
    public async Task AddTransactionAsync(FinancialTransaction transaction, CancellationToken ct = default)
    {

        if (transaction == null) {
            throw new ArgumentNullException();
        }

        await _dbContext.FinancialTransactions.AddAsync(transaction, ct);

        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateStatusAsync(Guid transactionId, TransactionStatus status, RiskLevel riskLevel, DateTime processedAt , CancellationToken ct = default)
    {
        if(transactionId == Guid.Empty) {
            throw new ArgumentException("TransactionId cannot be empty");
        }
        
        var transaction = await _dbContext.FinancialTransactions.FirstOrDefaultAsync(t => t.Id == transactionId, ct);

        if (transaction is null)
        {
            // loga e retorna ou lança exception
            throw new InvalidOperationException(
                $"Transaction {transactionId} not found.");
        }

        transaction.Status = status;
        transaction.RiskLevel = riskLevel;
        transaction.ProcessedAt = processedAt;

        await _dbContext.SaveChangesAsync(ct);
    }


}
