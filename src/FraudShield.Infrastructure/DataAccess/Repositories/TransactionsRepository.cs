using FraudShield.Domain.Entities;
using FraudShield.Domain.Repositories.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraudShield.Infrastructure.DataAccess.Repositories;

internal class TransactionsRepository : ITransactionsWriteOnlyRepository
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
}
