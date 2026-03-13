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

    /// <summary>
    /// Asynchronously adds a new financial transaction to the system.
    /// </summary>
    /// <param name="transaction">The financial transaction to add. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the unique identifier of the added
    /// transaction.</returns>
    /// <exception cref="NotImplementedException">Thrown if the method is not implemented.</exception>
    public Task<Guid> AddTransactionAsync(FinancialTransaction transaction)
    {
        throw new NotImplementedException();
    }
}
