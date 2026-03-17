using FraudShield.Domain.Entities;
using FraudShield.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraudShield.Domain.Repositories.Transactions;

public interface ITransactionsUpdateOnlyRepository
{
    Task UpdateStatusAsync(Guid transactionId, TransactionStatus status, RiskLevel riskLevel, DateTime processedAt,  CancellationToken ct = default);
}
