using FraudShield.Communication.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraudShield.Application.UseCases.Transaction.GetById;

public interface IGetTransactionsByIdUseCase
{
    Task<ResponseShortTransactionJson> Execute(Guid transactionId, CancellationToken ct = default);
}
