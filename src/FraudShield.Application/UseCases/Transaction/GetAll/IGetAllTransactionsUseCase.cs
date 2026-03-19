using FraudShield.Communication.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraudShield.Application.UseCases.Transaction.GetAll;

public interface IGetAllTransactionsUseCase
{
    Task<ResponseTransactionsJson> Execute(CancellationToken ct = default);
}
