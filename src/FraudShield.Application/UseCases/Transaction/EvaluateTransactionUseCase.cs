using FraudShield.Communication.Requests;
using FraudShield.Communication.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraudShield.Application.UseCases.Transaction;

public class EvaluateTransactionUseCase : IEvaluateTransactionUseCase
{

    public EvaluateTransactionUseCase()
    {

    }

    public Task<ResponseEvaluateTransactionJson> ExecuteAsync(RequestEvaluateTransactionJson request)
    {
        throw new NotImplementedException();
    }
}
