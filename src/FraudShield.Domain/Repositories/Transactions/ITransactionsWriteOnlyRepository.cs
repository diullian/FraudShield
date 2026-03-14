using FraudShield.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraudShield.Domain.Repositories.Transactions;

public interface ITransactionsWriteOnlyRepository
{
    Task AddTransactionAsync(FinancialTransaction transaction);
}
