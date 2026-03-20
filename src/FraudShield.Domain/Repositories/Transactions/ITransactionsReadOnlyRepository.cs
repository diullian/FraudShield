using FraudShield.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraudShield.Domain.Repositories.Transactions;

public interface ITransactionsReadOnlyRepository
{
    Task<List<FinancialTransaction>> GetAll(CancellationToken ct = default);

    Task<FinancialTransaction?> GetById(Guid transactionId, CancellationToken ct = default);

    Task<bool> ExistsByCorrelationId(Guid CorrelationId, CancellationToken ct = default);
}
